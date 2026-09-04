using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Managers;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;
using System.Diagnostics;
using System.Threading.Channels;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Algorithms
{
    public class SteadyStateAsyncGA : IGeneticAlgorithm
    {
        protected GeneticAlgorithmParameters Parameters;
        protected TestFunction TestFunction;
        protected FitnessStats[] FitnessHistory;

        protected SelectionManager SelectionManager;
        protected CrossoverManager CrossoverManager;
        protected MutationManager MutationManager;

        protected readonly object PopulationLock = new();

        public GeneticAlgorithmResult Run(GeneticAlgorithmParameters parameters, IProgress<double>? progress, IProgress<MigrationTopologyReport>? topologyProgress, CancellationToken cancellationToken)
        {
            RandomProvider.Initialize(parameters.RandomSeed);
            Parameters = parameters;
            TestFunction = TestFunctionFactory.GetFunction(parameters.TestFunctionType);
            FitnessHistory = new FitnessStats[parameters.MaxGenerations + 1];

            SelectionManager = new SelectionManager(parameters);
            CrossoverManager = new CrossoverManager(parameters);
            MutationManager = new MutationManager(parameters, TestFunction);

            return RunAsync(progress, cancellationToken).GetAwaiter().GetResult();
        }

        protected async Task<GeneticAlgorithmResult> RunAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            Stopwatch timer = Stopwatch.StartNew();

            Population population = PopulationHelper.InitializePopulation(Parameters.PopulationSize, Parameters, TestFunction);
            PopulationHelper.EvaluateFitness(population, TestFunction);

            FitnessHistory[0] = PopulationHelper.GetFitnessSummary(population);
            Individual bestIndividual = PopulationHelper.GetBest(population);

            int stagnantGenerations = 0;
            int generationOfOptimum = 0;
            TimeSpan timeToOptimum = TimeSpan.Zero;

            int producerEvaluations = 0;
            int consumerEvaluations = 0;
            int maxEvaluations = Parameters.MaxGenerations * Parameters.PopulationSize;
            int maxStagnantEvaluations = Parameters.MaxStagnantGenerations * Parameters.PopulationSize;
            int batchSize = Parameters.BatchSize;
            int generation = 1;

            progress?.Report(0);

            if (maxEvaluations % 2 != 0)
            {
                producerEvaluations++;

                SelectionManager.Preprocess(population);
                (Individual parent1, Individual parent2) = SelectionManager.SelectParents(population);
                (Individual child, _) = CrossoverManager.ApplyCrossover(parent1, parent2);
                MutationManager.Mutate(child);
                await PopulationHelper.EvaluateFitnessAsync(child, TestFunction, cancellationToken);

                lock (PopulationLock)
                {
                    consumerEvaluations++;
                    int worstIndex = PopulationHelper.GetWorstIndex(population.Individuals);

                    if (child.Fitness < population.Individuals[worstIndex].Fitness)
                    {
                        population.Individuals[worstIndex] = child;
                    }
                }
            }

            Channel<Individual[]> childChannel = Channel.CreateBounded<Individual[]>(new BoundedChannelOptions(batchSize)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });

            Task producer = Task.Run(async () =>
            {
                while (producerEvaluations < maxEvaluations && !cancellationToken.IsCancellationRequested)
                {
                    producerEvaluations += batchSize;
                    Individual[] batch = new Individual[batchSize];
                    int pairs = batchSize / 2;

                    SelectionManager.Preprocess(population);

                    for (int i = 0; i < pairs; i++)
                    {
                        (Individual parent1, Individual parent2) = SelectionManager.SelectParents(population);
                        (Individual child1, Individual child2) = CrossoverManager.ApplyCrossover(parent1, parent2);
                        MutationManager.Mutate(child1);
                        MutationManager.Mutate(child2);

                        batch[i * 2] = child1;
                        batch[i * 2 + 1] = child2;
                    }

                    if (batchSize % 2 == 1)
                    {
                        (Individual parent1, Individual parent2) = SelectionManager.SelectParents(population);
                        (Individual child, _) = CrossoverManager.ApplyCrossover(parent1, parent2);
                        MutationManager.Mutate(child);
                        batch[batchSize - 1] = child;
                    }

                    await childChannel.Writer.WriteAsync(batch, cancellationToken);
                }

                childChannel.Writer.Complete();
            }, cancellationToken);

            await foreach (var children in childChannel.Reader.ReadAllAsync(cancellationToken))
            {
                await PopulationHelper.EvaluateFitnessAsync(new Population(children), TestFunction, cancellationToken);

                lock (PopulationLock)
                {
                    consumerEvaluations += children.Length;

                    for (int i = 0; i < children.Length; i += 2)
                    {
                        if (i + 1 < children.Length)
                        {
                            PopulationHelper.TryReplaceWorst(population.Individuals, children[i], children[i + 1]);
                        }
                        else
                        {
                            int worstIndex = PopulationHelper.GetWorstIndex(population.Individuals);
                            if (children[i].Fitness < population.Individuals[worstIndex].Fitness)
                            {
                                population.Individuals[worstIndex] = children[i];
                            }
                        }
                    }

                    Individual currentBest = PopulationHelper.GetBest(population);
                    double improvement = double.MaxValue;

                    if (currentBest.Fitness < bestIndividual.Fitness)
                    {
                        stagnantGenerations = 0;
                        improvement = Math.Abs(bestIndividual.Fitness - currentBest.Fitness);
                        timeToOptimum = timer.Elapsed;
                        generationOfOptimum = generation;
                        bestIndividual = currentBest.Clone();
                    }
                    else
                    {
                        stagnantGenerations++;
                    }

                    if (consumerEvaluations >= Parameters.PopulationSize)
                    {
                        consumerEvaluations -= Parameters.PopulationSize;
                        FitnessHistory[generation] = PopulationHelper.GetFitnessSummary(population);
                        progress?.Report(Math.Round((generation + 1) * 100.0 / Parameters.MaxGenerations, 2, MidpointRounding.AwayFromZero));
                        generation++;
                    }

                    if (cancellationToken.IsCancellationRequested || Parameters.PrecisionLimit && improvement < Parameters.PrecisionThreshold || Parameters.StagnantLimit && stagnantGenerations >= maxStagnantEvaluations)
                    {
                        break;
                    }
                }
            }

            if (generation <= Parameters.MaxGenerations && FitnessHistory[generation].Min == 0 && FitnessHistory[generation].Max == 0 && FitnessHistory[generation].Avg == 0)
            {
                FitnessHistory[generation] = PopulationHelper.GetFitnessSummary(population);
            }

            timer.Stop();
            progress?.Report(100);

            return new GeneticAlgorithmResult(population, bestIndividual, FitnessHistory, timer.Elapsed, timeToOptimum, generationOfOptimum, cancellationToken.IsCancellationRequested);
        }
    }
}