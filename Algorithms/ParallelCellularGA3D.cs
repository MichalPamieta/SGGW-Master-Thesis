using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Managers;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;
using System.Diagnostics;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Algorithms
{
    public class ParallelCellularGA3D : IGeneticAlgorithm
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

            int popSize = Parameters.PopulationSize;
            (int rows, int cols, int depth) = NeighborhoodHelper.FindBest3DGridShape(popSize);
            Individual[,,] grid = new Individual[rows, cols, depth];

            Population population = PopulationHelper.InitializePopulationParallel(popSize, Parameters, TestFunction, Parameters.ThreadCount);
            PopulationHelper.EvaluateFitnessParallel(population, TestFunction, Parameters.ThreadCount);

            for (int index = 0, x = 0; x < rows && index < population.Individuals.Length; x++)
            {
                for (int y = 0; y < cols && index < population.Individuals.Length; y++)
                {
                    for (int z = 0; z < depth && index < population.Individuals.Length; z++)
                    {
                        grid[x, y, z] = population.Individuals[index++];
                    }
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
                Individual[,,] nextGrid = new Individual[rows, cols, depth];

                Parallel.For(0, rows, new ParallelOptions { MaxDegreeOfParallelism = Parameters.ThreadCount }, x =>
                {
                    for (int y = 0; y < cols; y++)
                    {
                        for (int z = 0; z < depth; z++)
                        {
                            if (grid[x, y, z] == null)
                            {
                                continue;
                            }

                            Individual center = grid[x, y, z].Clone();
                            List<Individual> neighbors = NeighborhoodHelper.GetNeighbors3D(x, y, z, grid, Parameters.NeighborhoodType, Parameters.NeighborhoodRadius, Parameters.WrapNeighborhood);

                            if (neighbors.Count == 0)
                            {
                                continue;
                            }

                            int attempts = 5;
                            (Individual parent1, Individual parent2) = SelectionManager.SelectParents(neighbors);

                            while (parent1 == parent2 && neighbors.Count > 1 && attempts-- > 0)
                            {
                                (parent1, parent2) = SelectionManager.SelectParents(neighbors);
                            }

                            if (Parameters.CenterAlwaysParent)
                            {
                                if (center != parent1 && center != parent2)
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
                                nextGrid[x, y, z] = child.Fitness < center.Fitness ? child : center;
                            }
                            else
                            {
                                nextGrid[x, y, z] = child;
                            }
                        }
                    }
                });

                grid = nextGrid;

                List<Individual> flattened = [];
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        for (int z = 0; z < depth; z++)
                        {
                            if (grid[i, j, z] != null)
                            {
                                flattened.Add(grid[i, j, z]);
                            }
                        }
                    }
                }

                population = new Population([.. flattened]);
                if (!Parameters.ReplaceOnlyIfBetter)
                {
                    PopulationHelper.EvaluateFitnessParallel(population, TestFunction, Parameters.ThreadCount);
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
