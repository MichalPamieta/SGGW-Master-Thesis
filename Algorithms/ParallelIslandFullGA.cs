using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Managers;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;
using System.Diagnostics;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Algorithms
{
    public class ParallelIslandFullGA : IGeneticAlgorithm
    {
        protected GeneticAlgorithmParameters Parameters;
        protected TestFunction TestFunction;
        protected FitnessStats[] FitnessHistory;
        protected IslandManager IslandManager;

        public GeneticAlgorithmResult Run(GeneticAlgorithmParameters parameters, IProgress<double>? progress, IProgress<MigrationTopologyReport>? topologyProgress, CancellationToken cancellationToken)
        {
            RandomProvider.Initialize(parameters.RandomSeed);
            Parameters = parameters;
            TestFunction = TestFunctionFactory.GetFunction(parameters.TestFunctionType);
            FitnessHistory = new FitnessStats[parameters.MaxGenerations + 1];
            IslandManager = new IslandManager(parameters, TestFunction);

            return RunAlgorithm(progress, topologyProgress, cancellationToken);
        }
        protected GeneticAlgorithmResult RunAlgorithm(IProgress<double>? progress = null, IProgress<MigrationTopologyReport>? topologyProgress = null, CancellationToken cancellationToken = default)
        {
            Stopwatch timer = Stopwatch.StartNew();

            int[] threadsPerIsland = new int[Parameters.IslandCount];

            int remaining = Math.Max(0, Parameters.ThreadCount - Parameters.IslandCount);
            int baseExtra = remaining / Parameters.IslandCount;
            int extra = remaining % Parameters.IslandCount;

            for (int i = 0; i < Parameters.IslandCount; i++)
            {
                threadsPerIsland[i] = 1 + baseExtra + (i < extra ? 1 : 0);
            }

            Island[] islands = new Island[Parameters.IslandCount];
            int basePopSize = Parameters.PopulationSize / Parameters.IslandCount;
            int extraPopSize = Parameters.PopulationSize % Parameters.IslandCount;

            Parallel.For(0, Parameters.IslandCount, new ParallelOptions { MaxDegreeOfParallelism = Parameters.ThreadCount }, i =>
            {
                int currentPopSize = basePopSize + (i < extraPopSize ? 1 : 0);
                Population population = PopulationHelper.InitializePopulationParallel(currentPopSize, Parameters, TestFunction, threadsPerIsland[i]);
                PopulationHelper.EvaluateFitnessParallel(population, TestFunction, threadsPerIsland[i]);

                Island island = new(i, population, Parameters);
                island.AddFitnessStats(0);
                island.TryUpdateLocalBest(0, timer);
                islands[i] = island;
            });

            (FitnessStats globalStats, Individual globalBest) = IslandManager.CollectLastFitnessAndBestParallel(0, islands, Parameters.ThreadCount);
            FitnessHistory[0] = globalStats;

            Individual bestIndividual = globalBest.Clone();
            int stagnantGenerations = 0;
            int generationOfOptimum = 0;
            TimeSpan timeToOptimum = TimeSpan.Zero;

            string topologyName = IslandManager.MigrationTopologyName;
            Dictionary<int, int[]> migrationTopology = [];
            MigrationTopologyReport topologyReport = new(topologyName, migrationTopology);

            if (Parameters.MigrationType == MigrationType.None)
            {
                topologyProgress?.Report(topologyReport);
            }

            progress?.Report(0);

            for (int generation = 1; generation <= Parameters.MaxGenerations; generation++)
            {
                Parallel.For(0, islands.Length, new ParallelOptions { MaxDegreeOfParallelism = Parameters.IslandCount }, i =>
                {
                    IslandManager.EvolveIslandParallel(generation, islands[i], timer, threadsPerIsland[i]);
                });

                if (Parameters.MigrationType != MigrationType.None && generation % Parameters.MigrationFrequency == 0)
                {
                    IslandManager.GenerateMigrationTopology();

                    migrationTopology = IslandManager.GetMigrationTopology();
                    topologyReport = new(topologyName, migrationTopology);
                    topologyProgress?.Report(topologyReport);

                    IslandManager.PerformMigrationParallel(islands, Parameters.ThreadCount);
                }

                (FitnessStats currentStats, Individual currentBest) = IslandManager.CollectLastFitnessAndBestParallel(generation, islands, Parameters.ThreadCount);
                FitnessHistory[generation] = currentStats;

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

            Population finalPopulation = IslandManager.MergeIslands(islands);
            IslandResult[] finalIslands = IslandManager.GetIslandResults(islands);

            progress?.Report(100);

            return new GeneticAlgorithmResult(finalPopulation, bestIndividual, FitnessHistory, timer.Elapsed, timeToOptimum, generationOfOptimum, cancellationToken.IsCancellationRequested, finalIslands, topologyName, migrationTopology);
        }
    }
}
