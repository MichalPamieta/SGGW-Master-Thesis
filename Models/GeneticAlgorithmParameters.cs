namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models
{
    public class GeneticAlgorithmParameters
    {
        public int MaxGenerations { get; set; } = 2000;
        public int PopulationSize { get; set; } = 5000;
        public int GenotypeLength { get; set; } = 200;
        public int? RandomSeed { get; set; } = null;
        public bool StagnantLimit { get; set; } = false;
        public int MaxStagnantGenerations { get; set; } = 10;
        public bool PrecisionLimit { get; set; } = false;
        public double PrecisionThreshold { get; set; } = 1;
        public TestFunctionType TestFunctionType { get; set; } = TestFunctionType.Levy;
        public ExecutionModel ExecutionModel { get; set; } = ExecutionModel.Sequential;
        public int ThreadCount { get; set; } = Environment.ProcessorCount;

        public bool UseElitism { get; set; } = true;
        public ElitismType ElitismType { get; set; } = ElitismType.Insertion;
        public ElitismValueType ElitismValueType { get; set; } = ElitismValueType.Percentage;
        public int EliteCount { get; set; } = 10;
        public double ElitePercentage { get; set; } = 0.01;

        public SelectionType SelectionType { get; set; } = SelectionType.Tournament;
        public int TournamentSize { get; set; } = 3;
        public double SelectionPressureLinear { get; set; } = 1.5;
        public double SelectionPressureExponential { get; set; } = 1.5;
        public double TruncationFraction { get; set; } = 0.3;

        public CrossoverType CrossoverType { get; set; } = CrossoverType.OnePoint;
        public double CrossoverRate { get; set; } = 0.8;
        public int MultiPointCrossoverPoints { get; set; } = 3;

        public MutationType MutationType { get; set; } = MutationType.Random;
        public double MutationProbability { get; set; } = 0.05;
        public double GeneMutationProbability { get; set; } = 0.01;
        public double GaussianSigma { get; set; } = 1.0;

        public int BatchSize { get; set; } = 2;
        public double ProducerRatio { get; set; } = 0.5;

        public int IslandCount { get; set; } = 4;
        public MigrationType MigrationType { get; set; } = MigrationType.None;
        public MigrationTopologyType MigrationTopologyType { get; set; } = MigrationTopologyType.RingBidirectional;
        public double MigrationRate { get; set; } = 0.05;
        public int MigrationFrequency { get; set; } = 20;
        public int StarCenterId { get; set; } = 0;
        public int Offset { get; set; } = 0;
        public int OffsetCount { get; set; } = 2;

        public NeighborhoodType NeighborhoodType { get; set; } = NeighborhoodType.VonNeumann;
        public int NeighborhoodRadius { get; set; } = 1;
        public bool WrapNeighborhood { get; set; } = true;
        public bool CenterAlwaysParent { get; set; } = true;
        public bool ReplaceOnlyIfBetter {  get; set; } = true;
        public int GeneticDriftFrequency { get; set; } = 20;
        public double GeneticDriftProbability { get; set; } = 0.05;
    }
}
