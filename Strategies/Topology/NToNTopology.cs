namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Topology
{
    public class NToNTopology(int offset, int offsetCount) : IMigrationTopology
    {
        private int count;
        private readonly int offset = offset;
        private readonly int offsetCount = offsetCount;
        private int[][]? cachedTargets;
        private readonly object initLock = new();

        public void Initialize(int numberOfIslands)
        {
            lock (initLock)
            {
                if (cachedTargets != null)
                {
                    return;
                }

                count = Math.Max(1, numberOfIslands);
                cachedTargets = new int[count][];

                for (int source = 0; source < count; source++)
                {
                    if (count == 1)
                    {
                        cachedTargets[source] = [source];
                        continue;
                    }

                    List<int> targets = [];
                    for (int i = 0; i < offsetCount; i++)
                    {
                        int target = (source + offset + i + count) % count;
                        if (target != source)
                            targets.Add(target);
                    }

                    cachedTargets[source] = [.. targets];
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
