using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Topology;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Migration
{
    public interface IMigrationStrategy
    {
        void Migrate(Island[] islands, IMigrationTopology topology);
        void MigrateParallel(Island[] islands, IMigrationTopology topology, int threadCount);
    }
}
