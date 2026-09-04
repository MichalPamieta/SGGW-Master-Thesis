namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models
{
    public class MigrationTopologyReport(string topologyName, Dictionary<int, int[]> migrationTopology)
    {
        public string? TopologyName { get; set; } = topologyName;
        public Dictionary<int, int[]>? MigrationTopology { get; set; } = migrationTopology;
    }
}
