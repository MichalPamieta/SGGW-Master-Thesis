using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Topology;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;
using System.Collections.Concurrent;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Migration
{
    public class ExchangeRandomsMigration : IMigrationStrategy
    {
        public void Migrate(Island[] islands, IMigrationTopology topology)
        {
            Dictionary<int, List<(int, Individual)>> incomingMigrants = [];

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
                int[] sourceIndices = GetRandomIndices(sourceSize, migrantsCount);

                Individual[] sourceSelected = new Individual[migrantsCount];
                for (int j = 0; j < migrantsCount; j++)
                {
                    sourceSelected[j] = sourceIsland.Population.Individuals[sourceIndices[j]].Clone();
                }

                foreach (int targetId in targets)
                {
                    Island targetIsland = islands[targetId];
                    int targetSize = targetIsland.Population.Individuals.Length;
                    int exchangeCount = Math.Min(migrantsCount, targetSize);
                    int[] targetIndices = GetRandomIndices(targetSize, exchangeCount);

                    List<(int, Individual)> migrantsForTarget = new(exchangeCount);
                    for (int k = 0; k < exchangeCount; k++)
                    {
                        migrantsForTarget.Add((targetIndices[k], sourceSelected[k]));
                    }

                    if (!incomingMigrants.TryGetValue(targetId, out var existingList))
                    {
                        incomingMigrants[targetId] = migrantsForTarget;
                    }
                    else
                    {
                        existingList.AddRange(migrantsForTarget);
                    }
                }
            }

            foreach (var kvp in incomingMigrants)
            {
                Island targetIsland = islands[kvp.Key];
                var migrantsList = kvp.Value;

                foreach (var (index, individual) in migrantsList)
                {
                    targetIsland.Population.Individuals[index] = individual;
                }
            }
        }

        public void MigrateParallel(Island[] islands, IMigrationTopology topology, int threadCount)
        {
            ConcurrentDictionary<int, ConcurrentBag<(int, Individual)>> incomingMigrants = new();

            Parallel.For(0, islands.Length, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, i =>
            {
                Island sourceIsland = islands[i];
                int[] targets = topology.GetTargetIslands(sourceIsland.Id);

                if (targets.Length == 0)
                {
                    return;
                }

                int migrantsCount;
                Individual[] sourceSelected;

                lock (sourceIsland.LockObject)
                {
                    int sourceSize = sourceIsland.Population.Individuals.Length;
                    migrantsCount = Math.Min((int)(sourceIsland.Parameters.MigrationRate * sourceSize), sourceSize);

                    int[] sourceIndices = GetRandomIndices(sourceSize, migrantsCount);
                    sourceSelected = new Individual[migrantsCount];
                    for (int k = 0; k < migrantsCount; k++)
                    {
                        sourceSelected[k] = sourceIsland.Population.Individuals[sourceIndices[k]].Clone();
                    }
                }

                foreach (int targetId in targets)
                {
                    int targetSize;
                    int exchangeCount;
                    int[] targetIndices;

                    lock (islands[targetId].LockObject)
                    {
                        targetSize = islands[targetId].Population.Individuals.Length;
                        exchangeCount = Math.Min(migrantsCount, targetSize);
                        targetIndices = GetRandomIndices(targetSize, exchangeCount);
                    }

                    var bag = incomingMigrants.GetOrAdd(targetId, _ => []);
                    for (int k = 0; k < exchangeCount; k++)
                    {
                        bag.Add((targetIndices[k], sourceSelected[k]));
                    }
                }
            });

            Parallel.ForEach(incomingMigrants, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, kvp =>
            {
                int targetId = kvp.Key;
                Island targetIsland = islands[targetId];
                var migrantsBag = kvp.Value;

                lock (targetIsland.LockObject)
                {
                    foreach (var (index, individual) in migrantsBag)
                    {
                        targetIsland.Population.Individuals[index] = individual;
                    }
                }
            });
        }

        public void MigrateSafe(Island[] islands, IMigrationTopology topology)
        {
            Dictionary<int, List<(int, Individual)>> incomingMigrants = [];

            for (int i = 0; i < islands.Length; i++)
            {
                Island sourceIsland = islands[i];
                int[] targets = topology.GetTargetIslands(sourceIsland.Id);

                if (targets.Length == 0)
                {
                    continue;
                }

                int migrantsCount;
                Individual[] sourceSelected;

                lock (sourceIsland.LockObject)
                {
                    int sourceSize = sourceIsland.Population.Individuals.Length;
                    migrantsCount = Math.Min((int)(sourceIsland.Parameters.MigrationRate * sourceSize), sourceSize);
                    int[] sourceIndices = GetRandomIndices(sourceSize, migrantsCount);

                    sourceSelected = new Individual[migrantsCount];
                    for (int k = 0; k < migrantsCount; k++)
                    {
                        sourceSelected[k] = sourceIsland.Population.Individuals[sourceIndices[k]].Clone();
                    }
                }

                foreach (int targetId in targets)
                {
                    Island targetIsland = islands[targetId];
                    int targetSize;
                    int exchangeCount;
                    int[] targetIndices;

                    lock (targetIsland.LockObject)
                    {
                        targetSize = targetIsland.Population.Individuals.Length;
                        exchangeCount = Math.Min(migrantsCount, targetSize);
                        targetIndices = GetRandomIndices(targetSize, exchangeCount);
                    }

                    List<(int, Individual)> migrantsForTarget = new(exchangeCount);
                    for (int k = 0; k < exchangeCount; k++)
                    {
                        migrantsForTarget.Add((targetIndices[k], sourceSelected[k]));
                    }

                    if (!incomingMigrants.TryGetValue(targetId, out var existingList))
                    {
                        incomingMigrants[targetId] = migrantsForTarget;
                    }
                    else
                    {
                        existingList.AddRange(migrantsForTarget);
                    }
                }
            }

            foreach (var kvp in incomingMigrants)
            {
                Island targetIsland = islands[kvp.Key];
                var migrantsList = kvp.Value;

                lock (targetIsland.LockObject)
                {
                    foreach (var (index, individual) in migrantsList)
                    {
                        targetIsland.Population.Individuals[index] = individual;
                    }
                }
            }
        }

        public void MigrateParallelSimple(Island[] islands, IMigrationTopology topology, int threadCount)
        {
            ConcurrentDictionary<int, List<(int, Individual)>> incomingMigrants = new();

            Parallel.For(0, islands.Length, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, i =>
            {
                Island sourceIsland = islands[i];
                int[] targets = topology.GetTargetIslands(sourceIsland.Id);

                if (targets.Length == 0)
                {
                    return;
                }

                int sourceSize;
                int migrantsCount;
                int[] sourceIndices;
                Individual[] sourceSelected;

                lock (sourceIsland.LockObject)
                {
                    sourceSize = sourceIsland.Population.Individuals.Length;
                    migrantsCount = Math.Min((int)(sourceIsland.Parameters.MigrationRate * sourceSize), sourceSize);

                    sourceIndices = GetRandomIndices(sourceSize, migrantsCount);
                    sourceSelected = new Individual[migrantsCount];
                    for (int k = 0; k < migrantsCount; k++)
                    {
                        sourceSelected[k] = sourceIsland.Population.Individuals[sourceIndices[k]].Clone();
                    }
                }

                foreach (int targetId in targets)
                {
                    Island targetIsland = islands[targetId];
                    int targetSize;
                    int exchangeCount;
                    int[] targetIndices;

                    lock (targetIsland.LockObject)
                    {
                        targetSize = targetIsland.Population.Individuals.Length;
                        exchangeCount = Math.Min(migrantsCount, targetSize);
                        targetIndices = GetRandomIndices(targetSize, exchangeCount);

                    }

                    var migrantsForTarget = new List<(int index, Individual individual)>(exchangeCount);
                    for (int k = 0; k < exchangeCount; k++)
                    {
                        migrantsForTarget.Add((targetIndices[k], sourceSelected[k]));
                    }

                    incomingMigrants.AddOrUpdate(targetId,
                        _ => migrantsForTarget,
                        (_, existing) =>
                        {
                            lock (existing)
                            {
                                existing.AddRange(migrantsForTarget);
                                return existing;
                            }
                        });
                }
            });

            Parallel.ForEach(incomingMigrants, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, kvp =>
            {
                int targetId = kvp.Key;
                Island targetIsland = islands[targetId];
                var migrantsList = kvp.Value;

                lock (targetIsland.LockObject)
                {
                    foreach (var (index, individual) in migrantsList)
                    {
                        targetIsland.Population.Individuals[index] = individual;
                    }
                }
            });
        }

        private static int[] GetRandomIndices(int max, int count)
        {
            if (count >= max / 2)
            {
                int[] indices = new int[max];
                for (int i = 0; i < max; i++)
                {
                    indices[i] = i;
                }

                for (int i = 0; i < count; i++)
                {
                    int j = RandomProvider.Next(i, max);
                    (indices[i], indices[j]) = (indices[j], indices[i]);
                }

                int[] result = new int[count];
                Array.Copy(indices, result, count);

                return result;
            }
            else
            {
                HashSet<int> used = [];
                int[] result = new int[count];
                int idx = 0;
                while (idx < count)
                {
                    int candidate = RandomProvider.Next(max);
                    if (used.Add(candidate))
                    {
                        result[idx++] = candidate;
                    }
                }

                return result;
            }
        }
    }
}