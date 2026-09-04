using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Topology;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;
using System.Collections.Concurrent;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Migration
{
    public class ExchangeElitesMigration : IMigrationStrategy
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

                int sourceSize = sourceIsland.Population.Individuals.Length;
                int migrantsCount = Math.Min((int)(sourceIsland.Parameters.MigrationRate * sourceSize), sourceSize);
                Individual[] migrants = PopulationHelper.GetElites(sourceIsland.Population, migrantsCount);

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
                int targetId = kvp.Key;
                List<Individual> migrants = kvp.Value;
                Island targetIsland = islands[targetId];
                
                int targetSize = targetIsland.Population.Individuals.Length;
                int count = Math.Min(migrants.Count, targetSize);

                for (int k = 0; k < count; k++)
                {
                    targetIsland.Population.Individuals[k] = migrants[k];
                }
            }
        }

        public void MigrateParallel(Island[] islands, IMigrationTopology topology, int threadCount)
        {
            int islandCount = islands.Length;
            ConcurrentDictionary<int, ConcurrentBag<Individual>> incomingMigrants = new();

            Parallel.For(0, islandCount, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, i =>
            {
                Island sourceIsland = islands[i];
                int[] targets = topology.GetTargetIslands(sourceIsland.Id);

                if (targets.Length == 0)
                {
                    return;
                }

                Individual[] migrants;

                lock (sourceIsland.LockObject)
                {
                    int sourceSize = sourceIsland.Population.Individuals.Length;
                    int migrantsCount = Math.Min((int)(sourceIsland.Parameters.MigrationRate * sourceSize), sourceSize);
                    migrants = PopulationHelper.GetElites(sourceIsland.Population, migrantsCount);
                }

                for (int j = 0; j < targets.Length; j++)
                {
                    int targetId = targets[j];
                    ConcurrentBag<Individual> bag = incomingMigrants.GetOrAdd(targetId, _ => []);
                    for (int k = 0; k < migrants.Length; k++)
                    {
                        bag.Add(migrants[k]);
                    }
                }
            });

            Parallel.ForEach(incomingMigrants, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, kvp =>
            {
                int targetId = kvp.Key;
                ConcurrentBag<Individual> migrants = kvp.Value;
                Island targetIsland = islands[targetId];

                lock (targetIsland.LockObject)
                {
                    int targetSize = targetIsland.Population.Individuals.Length;
                    int count = Math.Min(migrants.Count, targetSize);

                    int i = 0;
                    foreach (var migrant in migrants)
                    {
                        if (i >= count)
                        {
                            break;
                        }

                        targetIsland.Population.Individuals[i++] = migrant;
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

                Individual[] migrants;

                lock (sourceIsland.LockObject)
                {
                    int sourceSize = sourceIsland.Population.Individuals.Length;
                    int migrantsCount = Math.Min((int)(sourceIsland.Parameters.MigrationRate * sourceSize), sourceSize);
                    migrants = PopulationHelper.GetElites(sourceIsland.Population, migrantsCount);
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
                int targetId = kvp.Key;
                List<Individual> migrants = kvp.Value;
                Island targetIsland = islands[targetId];

                lock (targetIsland.LockObject)
                {
                    int targetSize = targetIsland.Population.Individuals.Length;
                    int count = Math.Min(migrants.Count, targetSize);

                    for (int k = 0; k < count; k++)
                    {
                        targetIsland.Population.Individuals[k] = migrants[k];
                    }
                }
            }
        }

        public void MigrateParallelSimple(Island[] islands, IMigrationTopology topology, int threadCount)
        {
            int islandCount = islands.Length;
            ConcurrentDictionary<int, List<Individual>> incomingMigrants = new();

            Parallel.For(0, islandCount, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, i =>
            {
                Island sourceIsland = islands[i];
                int[] targets = topology.GetTargetIslands(sourceIsland.Id);

                if (targets.Length == 0)
                {
                    return;
                }

                Individual[] migrants;

                lock (sourceIsland.LockObject)
                {
                    int sourceSize = sourceIsland.Population.Individuals.Length;
                    int migrantsCount = Math.Min((int)(sourceIsland.Parameters.MigrationRate * sourceSize), sourceSize);
                    migrants = PopulationHelper.GetElites(sourceIsland.Population, migrantsCount);
                }

                for (int j = 0; j < targets.Length; j++)
                {
                    int targetId = targets[j];
                    incomingMigrants.AddOrUpdate(targetId,
                        (_) =>
                        {
                            List<Individual> list = [.. migrants];
                            return list;
                        },
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
                int targetId = kvp.Key;
                List<Individual> migrants = kvp.Value;
                Island targetIsland = islands[targetId];

                lock (targetIsland.LockObject)
                {
                    int targetSize = targetIsland.Population.Individuals.Length;
                    int count = Math.Min(migrants.Count, targetSize);

                    for (int i = 0; i < count; i++)
                    {
                        targetIsland.Population.Individuals[i] = migrants[i];
                    }
                }
            });
        }
    }
}
