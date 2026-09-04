namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Topology
{
    public class GridTopology : IMigrationTopology
    {
        private int count;
        private int rows;
        private int cols;
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
                cols = (int)Math.Ceiling(Math.Sqrt(count));
                rows = (int)Math.Ceiling((double)count / cols);
                cachedTargets = new int[count][];

                for (int i = 0; i < count; i++)
                {
                    if (count < 2)
                    {
                        cachedTargets[i] = [i];
                        continue;
                    }

                    List<int> targets = [];
                    int row = i / cols;
                    int col = i % cols;

                    if (row > 0)
                    {
                        int up = i - cols;
                        if (up < count) targets.Add(up);
                    }

                    if (row < rows - 1)
                    {
                        int down = i + cols;
                        if (down < count) targets.Add(down);
                    }

                    if (col > 0)
                    {
                        int left = i - 1;
                        if (left < count) targets.Add(left);
                    }

                    if (col < cols - 1)
                    {
                        int right = i + 1;
                        if (right < count) targets.Add(right);
                    }

                    cachedTargets[i] = [.. targets];
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
