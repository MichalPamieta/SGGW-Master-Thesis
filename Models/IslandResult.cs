namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models
{
    public class IslandResult(Island island)
    {
        public int IslandId { get; set; } = island.Id;
        public Individual BestIndividual { get; set; } = island.LocalBest.Clone();
        public FitnessStats[] FitnessHistory { get; set; } = island.FitnessHistory;
        public TimeSpan? TimeToBest { get; set; } = island.TimeToBest;
        public int? BestGeneration { get; set; } = island.BestGeneration;
    }
}
