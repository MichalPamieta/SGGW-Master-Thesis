namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Topology
{
    public interface IMigrationTopology
    {
        void Initialize(int numberOfIslands);
        int[] GetTargetIslands(int sourceIslandId);
        Dictionary<int, int[]> GetTopology();
    }
}
