using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Topology
{
    public class TotalChaosTopology : IMigrationTopology
    {
        private int count;
        private int[][]? cachedTargets;
        private readonly object initLock = new();

        public void Initialize(int numberOfIslands)
        {
            lock (initLock)
            {
                count = Math.Max(1, numberOfIslands);
                cachedTargets = new int[count][];

                for (int source = 0; source < count; source++)
                {
                    if (count < 2)
                    {
                        cachedTargets[source] = [];
                        continue;
                    }

                    int candidatesCount = count - 1;
                    int targetCount = RandomProvider.Next(1, candidatesCount + 1);

                    int[] candidates = new int[candidatesCount];
                    int index = 0;
                    for (int i = 0; i < count; i++)
                    {
                        if (i != source)
                        {
                            candidates[index++] = i;
                        }
                    }

                    for (int i = 0; i < targetCount; i++)
                    {
                        int swapIndex = i + RandomProvider.Next(candidatesCount - i);
                        (candidates[swapIndex], candidates[i]) = (candidates[i], candidates[swapIndex]);
                    }

                    int[] targets = new int[targetCount];
                    Array.Copy(candidates, targets, targetCount);
                    cachedTargets[source] = targets;
                }
            }
        }

        public int[] GetTargetIslands(int sourceIslandId) => cachedTargets![sourceIslandId];

        public Dictionary<int, int[]> GetTopology()
        {
            Dictionary<int, int[]> map = [];
            for (int i = 0; i < count; i++)
            {
                map[i] = GetTargetIslands(i);
            }

            return map;
        }
    }
}
