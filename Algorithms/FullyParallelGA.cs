using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Managers;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;
using System.Diagnostics;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Algorithms
{
    public class FullyParallelGA : IGeneticAlgorithm
    {
        protected GeneticAlgorithmParameters Parameters;
        protected TestFunction TestFunction;
        protected FitnessStats[] FitnessHistory;

        protected ElitismManager ElitismManager;
        protected SelectionManager SelectionManager;
        protected CrossoverManager CrossoverManager;
        protected MutationManager MutationManager;

        public GeneticAlgorithmResult Run(GeneticAlgorithmParameters parameters, IProgress<double>? progress, IProgress<MigrationTopologyReport>? topologyProgress, CancellationToken cancellationToken)
        {
            RandomProvider.Initialize(parameters.RandomSeed);
            Parameters = parameters;
            TestFunction = TestFunctionFactory.GetFunction(parameters.TestFunctionType);
            FitnessHistory = new FitnessStats[parameters.MaxGenerations + 1];

            ElitismManager = new ElitismManager(parameters);
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

            Individual[] elites = [];
            int eliteCount = 0;

            if (Parameters.UseElitism)
            {
                ElitismManager.Preprocess(population.Individuals.Length);
                eliteCount = Parameters.ElitismType == ElitismType.Insertion ? ElitismManager.EliteCount : 0;
            }

            int childrenNeeded = Parameters.PopulationSize - eliteCount;
            int pairsToGenerate = childrenNeeded / 2;

            progress?.Report(0);

            for (int generation = 1; generation <= Parameters.MaxGenerations; generation++)
            {
                SelectionManager.Preprocess(population);

                if (Parameters.UseElitism)
                {
                    elites = ElitismManager.SelectElites(population);
                }

                Individual[] nextGeneration = new Individual[childrenNeeded];

                Parallel.For(0, pairsToGenerate, new ParallelOptions { MaxDegreeOfParallelism = Parameters.ThreadCount }, i =>
                {
                    (Individual parent1, Individual parent2) = SelectionManager.SelectParents(population);
                    (Individual child1, Individual child2) = CrossoverManager.ApplyCrossover(parent1, parent2);
                    MutationManager.Mutate(child1);
                    MutationManager.Mutate(child2);
                    nextGeneration[i * 2] = child1;
                    nextGeneration[i * 2 + 1] = child2;
                });

                if (childrenNeeded % 2 == 1)
                {
                    (Individual parent1, Individual parent2) = SelectionManager.SelectParents(population);
                    (Individual child, _) = CrossoverManager.ApplyCrossover(parent1, parent2);
                    MutationManager.Mutate(child);
                    nextGeneration[childrenNeeded - 1] = child;
                }

                population = new Population(nextGeneration);
                PopulationHelper.EvaluateFitnessParallel(population, TestFunction, Parameters.ThreadCount);

                if (Parameters.UseElitism)
                {
                    population = ElitismManager.ApplyElitism(population, elites);
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
