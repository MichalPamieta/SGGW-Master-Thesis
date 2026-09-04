using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Migration
{
    public static class MigrationStrategyFactory
    {
        public static IMigrationStrategy Create(GeneticAlgorithmParameters parameters)
        {
            return parameters.MigrationType switch
            {
                MigrationType.None => new NoneMigration(),
                MigrationType.ReplaceWorstWithBest => new ReplaceWorstWithBestMigration(),
                MigrationType.ReplaceWorstWithMixed => new ReplaceWorstWithMixedMigration(),
                MigrationType.ExchangeElites => new ExchangeElitesMigration(),
                MigrationType.ExchangeRandoms => new ExchangeRandomsMigration(),
                _ => throw new InvalidOperationException($"Unsupported selection type: {parameters.MigrationType}")
            };
        }
    }
}
