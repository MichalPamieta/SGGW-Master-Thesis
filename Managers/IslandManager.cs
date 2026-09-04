using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Migration;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Topology;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;
using System.Diagnostics;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Managers
{
    public class IslandManager(GeneticAlgorithmParameters parameters, TestFunction testFunction)
    {
        public string MigrationTopologyName = parameters.MigrationTopologyType.ToString();
        private readonly IMigrationTopology migrationTopology = MigrationTopologyFactory.Create(parameters);
        private readonly IMigrationStrategy migrationStrategy = MigrationStrategyFactory.Create(parameters);
        private readonly TestFunction testFunction = testFunction;
        private readonly int islandCount = parameters.IslandCount;

        public void GenerateMigrationTopology() => migrationTopology.Initialize(islandCount);

        public Dictionary<int, int[]> GetMigrationTopology() => migrationTopology.GetTopology();

        public void PerformMigration(Island[] islands)
        {
            migrationStrategy.Migrate(islands, migrationTopology);
        }

        public void PerformMigrationParallel(Island[] islands, int threadCount)
        {
            migrationStrategy.MigrateParallel(islands, migrationTopology, threadCount);
        }

        public static Population MergeIslands(Island[] islands)
        {
            int populationSize = 0;
            for (int i = 0; i < islands.Length; i++)
            {
                populationSize += islands[i].Population.Individuals.Length;
            }

            Individual[] finalIndividuals = new Individual[populationSize];
            int index = 0;
            for (int i = 0; i < islands.Length; i++)
            {
                Island island = islands[i];
                for (int j = 0; j < island.Population.Individuals.Length; j++)
                {
                    finalIndividuals[index++] = island.Population.Individuals[j];
                }
            }
            Population finalPopulation = new(finalIndividuals);

            return finalPopulation;
        }

        public static IslandResult[] GetIslandResults(Island[] islands)
        {
            IslandResult[] finalIslands = new IslandResult[islands.Length];
            for (int i = 0; i < islands.Length; i++)
            {
                finalIslands[i] = new IslandResult(islands[i]);
            }

            return finalIslands;
        }

        public void EvolveIsland(int generation, Island island, Stopwatch stopwatch, int threadsForFitness = 1)
        {
            lock (island.LockObject)
            {
                Population population = island.Population;

                island.SelectionManager.Preprocess(population);
                Individual[] elites = new Individual[island.EliteCount];
                int eliteCount = 0;

                if (island.Parameters.UseElitism)
                {
                    elites = island.ElitismManager.SelectElites(population);
                    eliteCount = island.Parameters.ElitismType == ElitismType.Insertion ? island.EliteCount : 0;
                }

                int childrenNeeded = island.Parameters.PopulationSize - eliteCount;
                int pairsToGenerate = childrenNeeded / 2;

                Individual[] nextGeneration = new Individual[childrenNeeded];

                for (int i = 0; i < pairsToGenerate; i++)
                {
                    (Individual parent1, Individual parent2) = island.SelectionManager.SelectParents(population);
                    (Individual child1, Individual child2) = island.CrossoverManager.ApplyCrossover(parent1, parent2);
                    island.MutationManager.Mutate(child1);
                    island.MutationManager.Mutate(child2);

                    nextGeneration[2 * i] = child1;
                    nextGeneration[2 * i + 1] = child2;
                }

                if (childrenNeeded % 2 == 1)
                {
                    (Individual parent1, Individual parent2) = island.SelectionManager.SelectParents(population);
                    (Individual child, _) = island.CrossoverManager.ApplyCrossover(parent1, parent2);
                    island.MutationManager.Mutate(child);
                    nextGeneration[childrenNeeded - 1] = child;
                }

                population = new Population(nextGeneration);

                if (threadsForFitness > 1)
                {
                    PopulationHelper.EvaluateFitnessParallel(population, testFunction, threadsForFitness);
                }
                else
                {
                    PopulationHelper.EvaluateFitness(population, testFunction);
                }

                if (island.Parameters.UseElitism)
                {
                    island.Population = island.ElitismManager.ApplyElitism(population, elites);
                }

                island.Population = population;
                island.AddFitnessStats(generation, PopulationHelper.GetFitnessSummary(population));
                island.TryUpdateLocalBest(generation, stopwatch);
            }
        }

        public void EvolveIslandParallel(int generation, Island island, Stopwatch stopwatch, int threadsForFitness = 1)
        {
            lock (island.LockObject)
            {
                Population population = island.Population;

                island.SelectionManager.Preprocess(population);
                Individual[] elites = new Individual[island.EliteCount];
                int eliteCount = 0;

                if (island.Parameters.UseElitism)
                {
                    elites = island.ElitismManager.SelectElites(population);
                    eliteCount = island.Parameters.ElitismType == ElitismType.Insertion ? island.EliteCount : 0;
                }

                int childrenNeeded = island.Parameters.PopulationSize - eliteCount;
                int pairsToGenerate = childrenNeeded / 2;

                Individual[] nextGeneration = new Individual[childrenNeeded];

                Parallel.For(0, pairsToGenerate, new ParallelOptions { MaxDegreeOfParallelism = threadsForFitness }, i =>
                {
                    (Individual parent1, Individual parent2) = island.SelectionManager.SelectParents(population);
                    (Individual child1, Individual child2) = island.CrossoverManager.ApplyCrossover(parent1, parent2);
                    island.MutationManager.Mutate(child1);
                    island.MutationManager.Mutate(child2);

                    nextGeneration[2 * i] = child1;
                    nextGeneration[2 * i + 1] = child2;
                });

                if (childrenNeeded % 2 == 1)
                {
                    (Individual parent1, Individual parent2) = island.SelectionManager.SelectParents(population);
                    (Individual child, _) = island.CrossoverManager.ApplyCrossover(parent1, parent2);
                    island.MutationManager.Mutate(child);
                    nextGeneration[childrenNeeded - 1] = child;
                }

                population = new Population(nextGeneration);

                if (threadsForFitness > 1)
                {
                    PopulationHelper.EvaluateFitnessParallel(population, testFunction, threadsForFitness);
                }
                else
                {
                    PopulationHelper.EvaluateFitness(population, testFunction);
                }

                if (island.Parameters.UseElitism)
                {
                    island.Population = island.ElitismManager.ApplyElitism(population, elites);
                }

                island.Population = population;
                island.AddFitnessStats(generation, PopulationHelper.GetFitnessSummary(population));
                island.TryUpdateLocalBest(generation, stopwatch);
            }
        }

        public static (FitnessStats stats, Individual best) CollectLastFitnessAndBest(int generation, Island[] islands)
        {
            int count = islands.Length;
            FitnessStats last = islands[0].FitnessHistory[generation];
            double globalMin = last.Min;
            double globalMax = last.Max;
            double avgSum = last.Avg;
            Individual globalBest = islands[0].LocalBest.Clone();

            for (int i = 1; i < count; i++)
            {
                last = islands[i].FitnessHistory[generation];
                Individual candidate = islands[i].LocalBest.Clone();

                if (last.Min < globalMin)
                {
                    globalMin = last.Min;
                }
                if (last.Max > globalMax)
                {
                    globalMax = last.Max;
                }

                avgSum += last.Avg;

                if (candidate.Fitness < globalBest.Fitness)
                {
                    globalBest = candidate.Clone();
                }
            }

            double globalAvg = count > 0 ? avgSum / count : 0;

            return (new FitnessStats(globalMin, globalMax, globalAvg), globalBest);
        }

        public static (FitnessStats stats, Individual best) CollectLastFitnessAndBestParallel(int generation, Island[] islands, int threadCount)
        {
            int count = islands.Length;
            FitnessStats last = islands[0].FitnessHistory[generation];
            double globalMin = last.Min;
            double globalMax = last.Max;
            double avgSum = last.Avg;
            Individual globalBest = islands[0].LocalBest.Clone();

            var localResults = new (double Min, double Max, double AvgSum, Individual Best)[count];

            Parallel.For(1, count, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, i =>
            {
                FitnessStats last = islands[i].FitnessHistory[generation];
                Individual candidate = islands[i].LocalBest.Clone();

                localResults[i] = (last.Min, last.Max, last.Avg, candidate);
            });

            for (int i = 1; i < count; i++)
            {
                var (Min, Max, AvgSum, Best) = localResults[i];

                if (Min < globalMin)
                {
                    globalMin = Min;
                }
                if (Max > globalMax)
                {
                    globalMax = Max;
                }

                avgSum += AvgSum;

                if (Best.Fitness < globalBest.Fitness)
                {
                    globalBest = Best;
                }
            }

            double globalAvg = count > 0 ? avgSum / count : 0;

            return (new FitnessStats(globalMin, globalMax, globalAvg), globalBest);
        }
    }
}
