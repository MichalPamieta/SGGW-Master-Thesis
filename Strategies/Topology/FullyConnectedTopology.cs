namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Topology
{
    public class FullyConnectedTopology : IMigrationTopology
    {
        private int count;
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

                if (count < 2)
                {
                    for (int i = 0; i < count; i++)
                    {
                        cachedTargets[i] = [i];
                    }
                }
                else
                {
                    for (int source = 0; source < count; source++)
                    {
                        int[] targets = new int[count - 1];
                        int index = 0;
                        for (int i = 0; i < count; i++)
                        {
                            if (i != source)
                            {
                                targets[index++] = i;
                            }
                        }
                        cachedTargets[source] = targets;
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
