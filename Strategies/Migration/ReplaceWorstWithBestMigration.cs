using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Topology;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;
using System.Collections.Concurrent;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Migration
{
    public class ReplaceWorstWithBestMigration : IMigrationStrategy
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
                Island targetIsland = islands[kvp.Key];
                var migrants = kvp.Value;

                Population targetPopulation = targetIsland.Population;
                int targetSize = targetPopulation.Individuals.Length;
                int replaceCount = Math.Min(migrants.Count, targetSize);

                PopulationHelper.SortByFitness(targetPopulation);

                for (int i = 0; i < replaceCount; i++)
                {
                    int index = targetSize - 1 - i;
                    targetPopulation.Individuals[index] = migrants[i];
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
                    int sourceSize = sourceIsland.Population.Individuals.Length;
                    migrantsCount = Math.Min((int)(sourceIsland.Parameters.MigrationRate * sourceSize), sourceSize);
                    migrants = PopulationHelper.GetElites(sourceIsland.Population, migrantsCount);
                }

                for (int j = 0; j < targets.Length; j++)
                {
                    int targetId = targets[j];
                    var bag = incomingMigrants.GetOrAdd(targetId, _ => []);
                    for (int k = 0; k < migrants.Length; k++)
                    {
                        bag.Add(migrants[k]);
                    }
                }
            });

            Parallel.ForEach(incomingMigrants, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, kvp =>
            {
                int targetId = kvp.Key;
                Island targetIsland = islands[targetId];

                lock (targetIsland.LockObject)
                {
                    Population targetPopulation = targetIsland.Population;
                    int targetSize = targetPopulation.Individuals.Length;

                    PopulationHelper.SortByFitness(targetPopulation);

                    int i = 0;
                    foreach (var migrant in kvp.Value)
                    {
                        if (i >= targetSize) break;
                        int index = targetSize - 1 - i;
                        targetPopulation.Individuals[index] = migrant;
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
                    int sourceSize = sourceIsland.Population.Individuals.Length;
                    migrantsCount = Math.Min((int)(sourceIsland.Parameters.MigrationRate * sourceSize), sourceSize);
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
                Island targetIsland = islands[kvp.Key];
                var migrants = kvp.Value;

                lock (targetIsland.LockObject)
                {
                    Population targetPopulation = targetIsland.Population;
                    int targetSize = targetPopulation.Individuals.Length;
                    int replaceCount = Math.Min(migrants.Count, targetSize);

                    PopulationHelper.SortByFitness(targetPopulation);

                    for (int i = 0; i < replaceCount; i++)
                    {
                        int index = targetSize - 1 - i;
                        targetPopulation.Individuals[index] = migrants[i];
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
					int sourceSize = sourceIsland.Population.Individuals.Length;
					migrantsCount = Math.Min((int)(sourceIsland.Parameters.MigrationRate * sourceSize), sourceSize);
					migrants = PopulationHelper.GetElites(sourceIsland.Population, migrantsCount);
				}

                for (int j = 0; j < targets.Length; j++)
                {
                    int targetId = targets[j];
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
                int targetId = kvp.Key;
                Island targetIsland = islands[targetId];

                lock (targetIsland.LockObject)
                {
                    Population targetPopulation = targetIsland.Population;
                    int targetSize = targetPopulation.Individuals.Length;
                    int replaceCount = Math.Min(kvp.Value.Count, targetSize);

                    PopulationHelper.SortByFitness(targetPopulation);

                    for (int i = 0; i < replaceCount; i++)
                    {
                        int index = targetSize - 1 - i;
                        targetPopulation.Individuals[index] = kvp.Value[i];
                    }
                }
            });
        }
    }
}
