using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Topology
{
    public static class MigrationTopologyFactory
    {
        public static IMigrationTopology Create(GeneticAlgorithmParameters parameters)
        {
            IMigrationTopology topology = parameters.MigrationTopologyType switch
            {
                MigrationTopologyType.RingUnidirectional => new RingUnidirectionalTopology(),
                MigrationTopologyType.RingBidirectional => new RingBidirectionalTopology(),
                MigrationTopologyType.FulllyConnected => new FullyConnectedTopology(),
                MigrationTopologyType.Grid => new GridTopology(),
                MigrationTopologyType.Star => new StarTopology(parameters.StarCenterId),
                MigrationTopologyType.Random => new RandomTopology(),
                MigrationTopologyType.Dynamic => new DynamicTopology(),
                MigrationTopologyType.TotalChaos => new TotalChaosTopology(),
                MigrationTopologyType.ControlledChaos => new ControlledChaosTopology(),
                MigrationTopologyType.OneToN => new OneToNTopology(),
                MigrationTopologyType.NToOne => new NToOneTopology(),
                MigrationTopologyType.NToN => new NToNTopology(parameters.Offset, parameters.OffsetCount),
                _ => throw new InvalidOperationException($"Unsupported migration topology type: {parameters.MigrationTopologyType}")
            };

            return topology;
        }
    }
}
