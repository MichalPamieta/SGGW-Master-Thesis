using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Managers;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;
using System.Diagnostics;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Algorithms
{
    public class DiffusionGA2D : IGeneticAlgorithm
    {
        protected GeneticAlgorithmParameters Parameters;
        protected TestFunction TestFunction;
        protected FitnessStats[] FitnessHistory;

        protected ElitismManager ElitismManager;
        protected SelectionManager SelectionManager;
        protected CrossoverManager CrossoverManager;
        protected MutationManager MutationManager;

        public GeneticAlgorithmResult Run(GeneticAlgorithmParameters parameters, IProgress<double>? progress, IProgress<MigrationTopologyReport>? topologyProgress, CancellationToken cancellationToken)
        {
            RandomProvider.Initialize(parameters.RandomSeed);
            Parameters = parameters;
            TestFunction = TestFunctionFactory.GetFunction(parameters.TestFunctionType);
            FitnessHistory = new FitnessStats[parameters.MaxGenerations + 1];

            ElitismManager = new ElitismManager(parameters);
            SelectionManager = new SelectionManager(parameters);
            CrossoverManager = new CrossoverManager(parameters);
            MutationManager = new MutationManager(parameters, TestFunction);

            return RunAlgorithm(progress, cancellationToken);
        }

        protected GeneticAlgorithmResult RunAlgorithm(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            Stopwatch timer = Stopwatch.StartNew();

            int popSize = Parameters.PopulationSize;
            (int rows, int cols) = NeighborhoodHelper.FindBest2DGridShape(popSize);

            Individual[,] grid = new Individual[rows, cols];
            Population population = PopulationHelper.InitializePopulation(popSize, Parameters, TestFunction);
            PopulationHelper.EvaluateFitness(population, TestFunction);

            for (int i = 0, index = 0; i < rows; i++)
            {
                for (int j = 0; j < cols && index < population.Individuals.Length; j++)
                {
                    grid[i, j] = population.Individuals[index++];
                }
            }

            FitnessHistory[0] = PopulationHelper.GetFitnessSummary(population);
            Individual bestIndividual = PopulationHelper.GetBest(population);

            int stagnantGenerations = 0;
            int generationOfOptimum = 0;
            TimeSpan timeToOptimum = TimeSpan.Zero;

            progress?.Report(0);

            for (int generation = 1; generation <= Parameters.MaxGenerations; generation++)
            {
                Individual[,] nextGrid = new Individual[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (grid[i, j] == null)
                        {
                            continue;
                        }

                        Individual center = grid[i, j].Clone();
                        List<Individual> neighbors = NeighborhoodHelper.GetNeighbors2D(i, j, grid, Parameters.NeighborhoodType, Parameters.NeighborhoodRadius, Parameters.WrapNeighborhood);

                        int attempts = 5;
                        (Individual parent1, Individual parent2) = SelectionManager.SelectParents(neighbors);

                        while (parent1 == parent2 && neighbors.Count > 1 && attempts-- > 0)
                        {
                            (parent1, parent2) = SelectionManager.SelectParents(neighbors);
                        }

                        if (Parameters.CenterAlwaysParent)
                        {
                            if (grid[i, j] != parent1 && grid[i, j] != parent2)
                            {
                                if (RandomProvider.Next(2) == 0)
                                {
                                    parent1 = center;
                                }
                                else
                                {
                                    parent2 = center;
                                }
                            }
                        }

                        (Individual child, _) = CrossoverManager.ApplyCrossover(parent1, parent2);
                        MutationManager.Mutate(child);

                        if (Parameters.ReplaceOnlyIfBetter)
                        {
                            PopulationHelper.EvaluateFitness(child, TestFunction);
                            nextGrid[i, j] = child.Fitness < center.Fitness ? child : center;
                        }
                        else
                        {
                            nextGrid[i, j] = child;
                        }
                    }
                }

                grid = nextGrid;

                List<Individual> flattened = [];
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (grid[i, j] != null)
                        {
                            flattened.Add(grid[i, j]);
                        }
                    }
                }

                Individual[] nextGeneration = [.. flattened];

                if (generation % Parameters.GeneticDriftFrequency == 0)
                {
                    Individual[] drift = new Individual[nextGeneration.Length];
                    for (int i = 0; i < drift.Length; i++)
                    {
                        drift[i] = nextGeneration[i].Clone();
                    }

                    for (int i = 0; i < nextGeneration.Length; i++)
                    {
                        if (RandomProvider.NextDouble() < Parameters.GeneticDriftProbability)
                        {
                            nextGeneration[RandomProvider.Next(nextGeneration.Length)] = drift[i];
                        }
                    }
                }

                population = new Population(nextGeneration);
                if (!Parameters.ReplaceOnlyIfBetter)
                {
                    PopulationHelper.EvaluateFitness(population, TestFunction);
                }

                FitnessHistory[generation] = PopulationHelper.GetFitnessSummary(population);

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

                progress?.Report(Math.Round((generation + 1) * 100.0 / Parameters.MaxGenerations, 2, MidpointRounding.AwayFromZero));

                if (cancellationToken.IsCancellationRequested || Parameters.PrecisionLimit && improvement < Parameters.PrecisionThreshold || Parameters.StagnantLimit && stagnantGenerations >= Parameters.MaxStagnantGenerations)
                {
                    break;
                }
            }

            timer.Stop();
            progress?.Report(100);

            return new GeneticAlgorithmResult(population, bestIndividual, FitnessHistory, timer.Elapsed, timeToOptimum, generationOfOptimum, cancellationToken.IsCancellationRequested);
        }
    }
}
