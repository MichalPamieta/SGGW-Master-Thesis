using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Managers;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;
using System.Diagnostics;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Algorithms
{
    public class IslandGA : IGeneticAlgorithm
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

            Island[] islands = new Island[Parameters.IslandCount];
            int basePopSize = Parameters.PopulationSize / Parameters.IslandCount;
            int extraPopSize = Parameters.PopulationSize % Parameters.IslandCount;

            for (int i = 0; i < Parameters.IslandCount; i++)
            {
                int currentPopSize = basePopSize + (i < extraPopSize ? 1 : 0);
                Population population = PopulationHelper.InitializePopulation(currentPopSize, Parameters, TestFunction);
                PopulationHelper.EvaluateFitness(population, TestFunction);

                Island island = new(i, population, Parameters);
                island.AddFitnessStats(0);
                island.TryUpdateLocalBest(0, timer);
                islands[i] = island;
            }

            (FitnessStats globalStats, Individual globalBest) = IslandManager.CollectLastFitnessAndBest(0, islands);
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
                for (int i = 0; i < islands.Length; i++)
                {
                    IslandManager.EvolveIsland(generation, islands[i], timer);
                }

                if (Parameters.MigrationType != MigrationType.None && generation % Parameters.MigrationFrequency == 0)
                {
                    IslandManager.GenerateMigrationTopology();

                    migrationTopology = IslandManager.GetMigrationTopology();
                    topologyReport = new(topologyName, migrationTopology);
                    topologyProgress?.Report(topologyReport);

                    IslandManager.PerformMigration(islands);
                }

                (FitnessStats currentStats, Individual currentBest) = IslandManager.CollectLastFitnessAndBest(generation, islands);
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
