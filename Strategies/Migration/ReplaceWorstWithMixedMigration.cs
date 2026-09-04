using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Topology;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;
using System.Collections.Concurrent;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Migration
{
    public class ReplaceWorstWithMixedMigration : IMigrationStrategy
    {
        public void Migrate(Island[] islands, IMigrationTopology topology)
        {
            Dictionary<int, List<Individual>> incomingMigrants = [];

            for (int i = 0; i < islands.Length; i++)
            {
                Island sourceIsland = islands[i];
                int[] targets = topology.GetTargetIslands(sourceIsland.Id);

                if (targets.Length == 0)
                {
                    continue;
                }

                Population sourcePopulation = sourceIsland.Population;
                int sourceSize = sourcePopulation.Individuals.Length;
                int migrantsCount = Math.Min((int)(sourceIsland.Parameters.MigrationRate * sourceSize), sourceSize);

                int elitesCount = migrantsCount / 2;
                int randomCount = migrantsCount - elitesCount;

                Individual[] eliteMigrants = PopulationHelper.GetElites(sourcePopulation, elitesCount);
                int[] randomIndices = GetRandomIndices(sourceSize, randomCount, elitesCount);

                Individual[] migrants = new Individual[migrantsCount];

                for (int j = 0; j < elitesCount; j++)
                {
                    migrants[j] = eliteMigrants[j];
                }

                for (int j = 0; j < randomCount; j++)
                {
                    migrants[elitesCount + j] = sourcePopulation.Individuals[randomIndices[j]].Clone();
                }

                for (int j = 0; j < targets.Length; j++)
                {
                    int targetId = targets[j];
                    if (!incomingMigrants.TryGetValue(targetId, out var existingList))
                    {
                        existingList = [];
                        incomingMigrants[targetId] = existingList;
                    }
                    existingList.AddRange(migrants);
                }
            }

            foreach (var kvp in incomingMigrants)
            {
                Island targetIsland = islands[kvp.Key];
                List<Individual> migrantsList = kvp.Value;

                Population targetPopulation = targetIsland.Population;
                int targetSize = targetPopulation.Individuals.Length;
                int replaceCount = Math.Min(migrantsList.Count, targetSize);

                PopulationHelper.SortByFitness(targetPopulation);

                for (int i = 0; i < replaceCount; i++)
                {
                    int worstIndex = targetSize - 1 - i;
                    targetPopulation.Individuals[worstIndex] = migrantsList[i];
                }
            }
        }

        public void MigrateParallel(Island[] islands, IMigrationTopology topology, int threadCount)
        {
            ConcurrentDictionary<int, ConcurrentBag<Individual>> incomingMigrants = new();

            Parallel.For(0, islands.Length, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, i =>
            {
                Island sourceIsland = islands[i];
                int[] targets = topology.GetTargetIslands(sourceIsland.Id);

                if (targets.Length == 0)
                {
                    return;
                }

                int migrantsCount;
                Individual[] migrants;

                lock (sourceIsland.LockObject)
                {
                    Population sourcePopulation = sourceIsland.Population;
                    int sourceSize = sourcePopulation.Individuals.Length;
                    migrantsCount = Math.Min((int)(sourceIsland.Parameters.MigrationRate * sourceSize), sourceSize);

                    int elitesCount = migrantsCount / 2;
                    int randomCount = migrantsCount - elitesCount;

                    Individual[] eliteMigrants = PopulationHelper.GetElites(sourcePopulation, elitesCount);
                    int[] randomIndices = GetRandomIndices(sourceSize, randomCount, elitesCount);

                    migrants = new Individual[migrantsCount];

                    for (int j = 0; j < elitesCount; j++)
                    {
                        migrants[j] = eliteMigrants[j];
                    }

                    for (int j = 0; j < randomCount; j++)
                    {
                        migrants[elitesCount + j] = sourcePopulation.Individuals[randomIndices[j]].Clone();
                    }
                }

                for (int j = 0; j < targets.Length; j++)
                {
                    int targetId = targets[j];
                    var bag = incomingMigrants.GetOrAdd(targetId, _ => new ConcurrentBag<Individual>());
                    for (int k = 0; k < migrants.Length; k++)
                    {
                        bag.Add(migrants[k]);
                    }
                }
            });

            Parallel.ForEach(incomingMigrants, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, kvp =>
            {
                Island targetIsland = islands[kvp.Key];
                var migrantsBag = kvp.Value;

                lock (targetIsland.LockObject)
                {
                    Population targetPopulation = targetIsland.Population;
                    int targetSize = targetPopulation.Individuals.Length;
                    int replaceCount = Math.Min(migrantsBag.Count, targetSize);

                    PopulationHelper.SortByFitness(targetPopulation);

                    int i = 0;
                    foreach (var migrant in migrantsBag)
                    {
                        if (i >= replaceCount) break;
                        int worstIndex = targetSize - 1 - i;
                        targetPopulation.Individuals[worstIndex] = migrant;
                        i++;
                    }
                }
            });
        }

        public void MigrateSafe(Island[] islands, IMigrationTopology topology)
        {
            Dictionary<int, List<Individual>> incomingMigrants = [];

            for (int i = 0; i < islands.Length; i++)
            {
                Island sourceIsland = islands[i];
                int[] targets = topology.GetTargetIslands(sourceIsland.Id);

                if (targets.Length == 0)
                {
                    continue;
                }

                int migrantsCount;
                Individual[] migrants;

                lock (sourceIsland.LockObject)
                {
                    Population sourcePopulation = sourceIsland.Population;
                    int sourceSize = sourcePopulation.Individuals.Length;
                    migrantsCount = Math.Min((int)(sourceIsland.Parameters.MigrationRate * sourceSize), sourceSize);

                    int elitesCount = migrantsCount / 2;
                    int randomCount = migrantsCount - elitesCount;

                    Individual[] eliteMigrants = PopulationHelper.GetElites(sourcePopulation, elitesCount);
                    int[] randomIndices = GetRandomIndices(sourceSize, randomCount, elitesCount);

                    migrants = new Individual[migrantsCount];

                    for (int j = 0; j < elitesCount; j++)
                    {
                        migrants[j] = eliteMigrants[j];
                    }

                    for (int j = 0; j < randomCount; j++)
                    {
                        migrants[elitesCount + j] = sourcePopulation.Individuals[randomIndices[j]].Clone();
                    }
                }

                for (int j = 0; j < targets.Length; j++)
                {
                    int targetId = targets[j];
                    if (!incomingMigrants.TryGetValue(targetId, out var existingList))
                    {
                        existingList = [];
                        incomingMigrants[targetId] = existingList;
                    }
                    existingList.AddRange(migrants);
                }
            }

            foreach (var kvp in incomingMigrants)
            {
                Island targetIsland = islands[kvp.Key];
                List<Individual> migrantsList = kvp.Value;

                lock (targetIsland.LockObject)
                {
                    Population targetPopulation = targetIsland.Population;
                    int targetPopulationSize = targetPopulation.Individuals.Length;
                    int replaceCount = Math.Min(migrantsList.Count, targetPopulationSize);

                    PopulationHelper.SortByFitness(targetPopulation);

                    for (int i = 0; i < replaceCount; i++)
                    {
                        int worstIndex = targetPopulationSize - 1 - i;
                        targetPopulation.Individuals[worstIndex] = migrantsList[i];
                    }
                }
            }
        }

        public void MigrateParallelSimple(Island[] islands, IMigrationTopology topology, int threadCount)
        {
            ConcurrentDictionary<int, List<Individual>> incomingMigrants = new();

            Parallel.For(0, islands.Length, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, i =>
            {
                Island sourceIsland = islands[i];
                int[] targets = topology.GetTargetIslands(sourceIsland.Id);

                if (targets.Length == 0)
                {
                    return;
                }

                int migrantsCount;
                Individual[] migrants;

                lock (sourceIsland.LockObject)
                {
                    Population sourcePopulation = sourceIsland.Population;
                    int sourceSize = sourcePopulation.Individuals.Length;
                    migrantsCount = Math.Min((int)(sourceIsland.Parameters.MigrationRate * sourceSize), sourceSize);

                    int elitesCount = migrantsCount / 2;
                    int randomCount = migrantsCount - elitesCount;

                    Individual[] eliteMigrants = PopulationHelper.GetElites(sourcePopulation, elitesCount);
                    int[] randomIndices = GetRandomIndices(sourceSize, randomCount, elitesCount);

                    migrants = new Individual[migrantsCount];

                    for (int j = 0; j < elitesCount; j++)
                    {
                        migrants[j] = eliteMigrants[j];
                    }

                    for (int j = 0; j < randomCount; j++)
                    {
                        migrants[elitesCount + j] = sourcePopulation.Individuals[randomIndices[j]].Clone();
                    }
                }

                foreach (int targetId in targets)
                {
                    incomingMigrants.AddOrUpdate(targetId,
                        _ => [.. migrants],
                        (_, existing) =>
                        {
                            lock (existing)
                            {
                                existing.AddRange(migrants);
                                return existing;
                            }
                        });
                }
            });

            Parallel.ForEach(incomingMigrants, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, kvp =>
            {
                Island targetIsland = islands[kvp.Key];
                List<Individual> migrantsList = kvp.Value;

                lock (targetIsland.LockObject)
                {
                    Population targetPopulation = targetIsland.Population;
                    int targetPopulationSize = targetPopulation.Individuals.Length;
                    int replaceCount = Math.Min(migrantsList.Count, targetPopulationSize);

                    PopulationHelper.SortByFitness(targetPopulation);

                    for (int i = 0; i < replaceCount; i++)
                    {
                        int worstIndex = targetPopulationSize - 1 - i;
                        targetPopulation.Individuals[worstIndex] = migrantsList[i];
                    }
                }
            });
        }

        private static int[] GetRandomIndices(int populationSize, int count, int excludeTopCount)
        {
            if (count <= 0)
            {
                return [];
            }

            HashSet<int> used = [];
            int[] result = new int[count];
            int filled = 0;

            while (filled < count)
            {
                int idx = RandomProvider.Next(excludeTopCount, populationSize);
                if (used.Add(idx))
                {
                    result[filled] = idx;
                    filled++;
                }
            }

            return result;
        }
    }
}
