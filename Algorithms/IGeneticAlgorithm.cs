using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Algorithms
{
    public interface IGeneticAlgorithm
    {
        GeneticAlgorithmResult Run(GeneticAlgorithmParameters geneticAlgorithmParameters, IProgress<double>? progress = null, IProgress<MigrationTopologyReport>? topologyProgress = null, CancellationToken cancellationToken = default);
    }
}
