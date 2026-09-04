using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Algorithms;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Managers;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;
using System.Diagnostics;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models
{
    public class Island
    {
        public int Id { get; }
        public Population Population { get; set; }
        public Individual LocalBest { get; private set; }
        public FitnessStats[] FitnessHistory { get; }
        public int EliteCount => ElitismManager.EliteCount;

        public GeneticAlgorithmParameters Parameters;
        public ElitismManager ElitismManager;
        public SelectionManager SelectionManager;
        public CrossoverManager CrossoverManager;
        public MutationManager MutationManager;
        public IGeneticAlgorithm? GeneticAlgorithm { get; }

        public TimeSpan? TimeToBest { get; private set; }
        public int? BestGeneration { get; private set; }

        public object LockObject { get; } = new();

        public Island(int id, Population population, GeneticAlgorithmParameters parameters)
        {
            Id = id;
            Population = population;
            Parameters = parameters;
            Parameters.PopulationSize = population.Individuals.Length;
            LocalBest = PopulationHelper.GetBest(Population);
            FitnessHistory = new FitnessStats[Parameters.MaxGenerations + 1];

            ElitismManager = new ElitismManager(Parameters);
            SelectionManager = new SelectionManager(Parameters);
            CrossoverManager = new CrossoverManager(Parameters);
            MutationManager = new MutationManager(Parameters, TestFunctionFactory.GetFunction(Parameters.TestFunctionType));

            if (Parameters.UseElitism)
            {
                ElitismManager.Preprocess(Parameters.PopulationSize);
            }
        }

        public void AddFitnessStats(int generation)
        {
            FitnessHistory[generation] = PopulationHelper.GetFitnessSummary(Population);
        }

        public void AddFitnessStats(int generation, FitnessStats fitnessStats)
        {
            FitnessHistory[generation] = fitnessStats;
        }

        public bool TryUpdateLocalBest(int currentGeneration, Stopwatch stopwatch)
        {
            Individual currentBest = PopulationHelper.GetBest(Population);
            if (currentBest.Fitness < LocalBest.Fitness)
            {
                LocalBest = currentBest;
                TimeToBest = stopwatch.Elapsed;
                BestGeneration = currentGeneration;

                return true;
            }

            return false;
        }
    }
}
