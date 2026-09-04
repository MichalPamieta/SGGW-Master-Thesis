namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models
{
    public class GeneticAlgorithmResult
    {
        public Population? Population { get; set; }
        public Individual BestIndividual { get; set; }
        public FitnessStats[] FitnessHistory { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan? BestTime { get; set; }
        public int? BestGeneration { get; set; }
        public bool WasCancelled { get; set; } = false;
        public IslandResult[]? IslandResults { get; set; }
        public string? TopologyName { get; set; }
        public Dictionary<int, int[]>? MigrationTopology { get; set; }

        public GeneticAlgorithmResult(Population population, Individual best, FitnessStats[] fitnessHistory, TimeSpan totalTime, TimeSpan bestTime, int bestGeneration, bool wasCancelled)
        {
            Population = population;
            BestIndividual = best;
            FitnessHistory = fitnessHistory;
            TotalTime = totalTime;
            BestTime = bestTime;
            BestGeneration = bestGeneration;
            WasCancelled = wasCancelled;
            IslandResults = [];
            TopologyName = null;
            MigrationTopology = null;
        }
        public GeneticAlgorithmResult(Population population, Individual best, FitnessStats[] fitnessHistory, TimeSpan totalTime, TimeSpan bestTime, int bestGeneration, bool wasCancelled, IslandResult[] islandResults, string topologyName, Dictionary<int, int[]> migrationTopology)
        {
            Population = population;
            BestIndividual = best;
            FitnessHistory = fitnessHistory;
            TotalTime = totalTime;
            BestTime = bestTime;
            BestGeneration = bestGeneration;
            WasCancelled = wasCancelled;
            IslandResults = islandResults;
            TopologyName= topologyName;
            MigrationTopology = migrationTopology;
        }
    }
}