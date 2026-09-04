namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models
{
    public enum ExecutionModel { Sequential, PartialParallel, FullyParallel, Island, ParallelIsland, ParallelIslandFull, Cellular1D, ParallelCellular1D, Cellular2D, ParallelCellular2D, Cellular3D, ParallelCellular3D, Diffusion1D,  ParallelDiffusion1D, Diffusion2D, ParallelDiffusion2D, Diffusion3D, ParallelDiffusion3D, SteadyState, SteadyStateAsync, ParallelSteadyStateAsync }
    public enum TestFunctionType { Ackley, Rastrigin, Rosenbrock, RosenbrockAltDomain, Griewank, Schwefel, Levy, Zakharov, SpinWaitL1Symmetric, SpinWaitL2Symmetric, SpinWaitL1Positive, SpinWaitL2Positive, SleepL1Symmetric, SleepL2Symmetric, SleepL1Positive, SleepL2Positive }
    public enum ElitismType { Insertion, Replacement }
    public enum ElitismValueType { Fixed, Percentage }
    public enum SelectionType { Random, Tournament, Roulette, SUS, RankSimple, RankLinear, RankExponential, Truncation }
    public enum CrossoverType { OnePoint, TwoPoint, MultiPoint, Uniform }
    public enum MutationType { Random, Gaussian }
    public enum MigrationType { None, ReplaceWorstWithBest, ReplaceWorstWithMixed, ExchangeElites, ExchangeRandoms }
    public enum MigrationTopologyType {  RingUnidirectional, RingBidirectional, FulllyConnected, Grid, Star, OneToN, NToOne, NToN, Random, Dynamic, ControlledChaos, TotalChaos }
    public enum NeighborhoodType { VonNeumann, Moore }
}
