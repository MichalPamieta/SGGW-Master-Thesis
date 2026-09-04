namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Topology
{
    public class StarTopology(int centerIslandId = 0) : IMigrationTopology
    {
        private int count;
        private readonly int centerId = centerIslandId;
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
                    if (count < 2)
                    {
                        cachedTargets[source] = [source];
                    }
                    else if (source == centerId)
                    {
                        int[] targets = new int[count - 1];
                        int index = 0;
                        for (int i = 0; i < count; i++)
                        {
                            if (i != centerId)
                            {
                                targets[index++] = i;
                            }
                        }

                        cachedTargets[source] = targets;
                    }
                    else
                    {
                        cachedTargets[source] = [centerId];
                    }
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
