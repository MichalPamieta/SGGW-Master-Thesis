using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Managers;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;
using System.Diagnostics;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Algorithms
{
    public class SteadyStateGA : IGeneticAlgorithm
    {
        protected GeneticAlgorithmParameters Parameters;
        protected TestFunction TestFunction;
        protected FitnessStats[] FitnessHistory;

        protected SelectionManager SelectionManager;
        protected CrossoverManager CrossoverManager;
        protected MutationManager MutationManager;

        public GeneticAlgorithmResult Run(GeneticAlgorithmParameters parameters, IProgress<double>? progress, IProgress<MigrationTopologyReport>? topologyProgress, CancellationToken cancellationToken)
        {
            RandomProvider.Initialize(parameters.RandomSeed);
            Parameters = parameters;
            TestFunction = TestFunctionFactory.GetFunction(parameters.TestFunctionType);
            FitnessHistory = new FitnessStats[parameters.MaxGenerations + 1];

            SelectionManager = new SelectionManager(parameters);
            CrossoverManager = new CrossoverManager(parameters);
            MutationManager = new MutationManager(parameters, TestFunction);

            return RunAlgorithm(progress, cancellationToken);
        }

        protected GeneticAlgorithmResult RunAlgorithm(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            Stopwatch timer = Stopwatch.StartNew();

            Population population = PopulationHelper.InitializePopulation(Parameters.PopulationSize, Parameters, TestFunction);
            PopulationHelper.EvaluateFitness(population, TestFunction);

            FitnessHistory[0] = PopulationHelper.GetFitnessSummary(population);
            Individual bestIndividual = PopulationHelper.GetBest(population);

            int stagnantGenerations = 0;
            int generationOfOptimum = 0;
            TimeSpan timeToOptimum = TimeSpan.Zero;

            int totalEvaluations = 0;
            int maxEvaluations = Parameters.MaxGenerations * Parameters.PopulationSize;
            int maxStagnantEvaluations = Parameters.MaxStagnantGenerations * Parameters.PopulationSize;
            int generation = 1;

            progress?.Report(0);

            if (maxEvaluations % 2 != 0)
            {
                totalEvaluations++;

                SelectionManager.Preprocess(population);
                (Individual parent1, Individual parent2) = SelectionManager.SelectParents(population);
                (Individual child, _) = CrossoverManager.ApplyCrossover(parent1, parent2);
                MutationManager.Mutate(child);
                PopulationHelper.EvaluateFitness(child, TestFunction);

                int worstIndex = PopulationHelper.GetWorstIndex(population.Individuals);
                if (child.Fitness < population.Individuals[worstIndex].Fitness)
                {
                    population.Individuals[worstIndex] = child;
                }
            }

            while (totalEvaluations < maxEvaluations)
            {
                totalEvaluations += 2;

                SelectionManager.Preprocess(population);
                (Individual parent1, Individual parent2) = SelectionManager.SelectParents(population);
                (Individual child1, Individual child2) = CrossoverManager.ApplyCrossover(parent1, parent2);
                MutationManager.Mutate(child1);
                MutationManager.Mutate(child2);

                PopulationHelper.EvaluateFitness(child1, TestFunction);
                PopulationHelper.EvaluateFitness(child2, TestFunction);

                PopulationHelper.TryReplaceWorst(population.Individuals, child1, child2);

                Individual currentBest = PopulationHelper.GetBest(population);
                double improvement = double.MaxValue;

                if (currentBest.Fitness < bestIndividual.Fitness)
                {
                    stagnantGenerations = 0;
                    improvement = Math.Abs(bestIndividual.Fitness - currentBest.Fitness);
                    timeToOptimum = timer.Elapsed;
                    generationOfOptimum = generation;
                    bestIndividual = currentBest.Clone();
                }
                else
                {
                    stagnantGenerations++;
                }

                if (totalEvaluations % Parameters.PopulationSize == 0)
                {
                    FitnessHistory[generation] = PopulationHelper.GetFitnessSummary(population);
                    progress?.Report(Math.Round((generation + 1) * 100.0 / Parameters.MaxGenerations, 2, MidpointRounding.AwayFromZero));
                    generation++;
                }

                if (cancellationToken.IsCancellationRequested || (Parameters.PrecisionLimit && improvement < Parameters.PrecisionThreshold) || (Parameters.StagnantLimit && stagnantGenerations >= maxStagnantEvaluations))
                {
                    break;
                }
            }

            if (generation <= Parameters.MaxGenerations && FitnessHistory[generation].Min == 0 && FitnessHistory[generation].Max == 0 && FitnessHistory[generation].Avg == 0)
            {
                FitnessHistory[generation] = PopulationHelper.GetFitnessSummary(population);
            }

            timer.Stop();
            progress?.Report(100);

            return new GeneticAlgorithmResult(population, bestIndividual, FitnessHistory, timer.Elapsed, timeToOptimum, generationOfOptimum, cancellationToken.IsCancellationRequested);
        }
    }
}
