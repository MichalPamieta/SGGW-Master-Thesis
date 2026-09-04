using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Managers;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;
using System.Diagnostics;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Algorithms
{
    public class ParallelCellularGA1D : IGeneticAlgorithm
    {
        protected GeneticAlgorithmParameters Parameters;
        protected TestFunction TestFunction;
        protected FitnessStats[] FitnessHistory;

        protected SelectionManager SelectionManager;
        protected CrossoverManager CrossoverManager;
        protected MutationManager MutationManager;

        public GeneticAlgorithmResult Run(GeneticAlgorithmParameters parameters, IProgress<double>? progress, IProgress<MigrationTopologyReport>? topologyProgress, CancellationToken cancellationToken)
        {
            RandomProvider.Initialize(parameters.RandomSeed);
            Parameters = parameters;
            TestFunction = TestFunctionFactory.GetFunction(parameters.TestFunctionType);
            FitnessHistory = new FitnessStats[parameters.MaxGenerations + 1];

            SelectionManager = new SelectionManager(parameters);
            CrossoverManager = new CrossoverManager(parameters);
            MutationManager = new MutationManager(parameters, TestFunction);

            return RunAlgorithm(progress, cancellationToken);
        }

        protected GeneticAlgorithmResult RunAlgorithm(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            Stopwatch timer = Stopwatch.StartNew();

            Population population = PopulationHelper.InitializePopulationParallel(Parameters.PopulationSize, Parameters, TestFunction, Parameters.ThreadCount);
            PopulationHelper.EvaluateFitnessParallel(population, TestFunction, Parameters.ThreadCount);

            FitnessHistory[0] = PopulationHelper.GetFitnessSummary(population);
            Individual bestIndividual = PopulationHelper.GetBest(population);

            int stagnantGenerations = 0;
            int generationOfOptimum = 0;
            TimeSpan timeToOptimum = TimeSpan.Zero;

            progress?.Report(0);

            for (int generation = 1; generation <= Parameters.MaxGenerations; generation++)
            {
                Individual[] currentPopulation = population.Individuals;
                Individual[] nextGeneration = new Individual[currentPopulation.Length];

                Parallel.For(0, currentPopulation.Length, new ParallelOptions { MaxDegreeOfParallelism = Parameters.ThreadCount }, i =>
                {
                    Individual center = currentPopulation[i].Clone();
                    List<Individual> neighborhood = NeighborhoodHelper.GetNeighborhood1D(currentPopulation, i, Parameters.NeighborhoodRadius, Parameters.WrapNeighborhood);

                    int attempts = 5;
                    (Individual parent1, Individual parent2) = SelectionManager.SelectParents(neighborhood);
                    while (parent1 == parent2 && neighborhood.Count > 1 && attempts-- > 0)
                    {
                        (parent1, parent2) = SelectionManager.SelectParents(neighborhood);
                    }

                    if (Parameters.CenterAlwaysParent)
                    {

                        if (center != parent1 && center != parent2)
                        {
                            if (RandomProvider.Next(2) == 0)
                            {
                                parent1 = center;
                            }
                            else
                            {
                                parent2 = center;
                            }
                        }
                    }

                    (Individual child, _) = CrossoverManager.ApplyCrossover(parent1, parent2);
                    MutationManager.Mutate(child);

                    if (Parameters.ReplaceOnlyIfBetter)
                    {
                        PopulationHelper.EvaluateFitness(child, TestFunction);
                        nextGeneration[i] = child.Fitness < center.Fitness ? child : center;
                    }
                    else
                    {
                        nextGeneration[i] = child;
                    }
                });

                population = new Population(nextGeneration);
                if (!Parameters.ReplaceOnlyIfBetter)
                {
                    PopulationHelper.EvaluateFitnessParallel(population, TestFunction, Parameters.ThreadCount);
                }

                FitnessHistory[generation] = PopulationHelper.GetFitnessSummary(population);
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

                progress?.Report(Math.Round((generation + 1) * 100.0 / Parameters.MaxGenerations, 2, MidpointRounding.AwayFromZero));

                if (cancellationToken.IsCancellationRequested || Parameters.PrecisionLimit && improvement < Parameters.PrecisionThreshold || Parameters.StagnantLimit && stagnantGenerations >= Parameters.MaxStagnantGenerations)
                {
                    break;
                }
            }

            timer.Stop();
            progress?.Report(100);

            return new GeneticAlgorithmResult(population, bestIndividual, FitnessHistory, timer.Elapsed, timeToOptimum, generationOfOptimum, cancellationToken.IsCancellationRequested);
        }
    }
}
