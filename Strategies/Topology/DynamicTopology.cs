using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Topology
{
    public class DynamicTopology : IMigrationTopology
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
                        cachedTargets[source] = [source];
                        continue;
                    }

                    int target = RandomProvider.Next(count);
                    while (target == source)
                    {
                        target = RandomProvider.Next(count);
                    }

                    cachedTargets[source] = [target];
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
