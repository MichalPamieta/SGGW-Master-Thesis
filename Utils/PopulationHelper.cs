using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils
{
    public static class PopulationHelper
    {
        public static Population InitializePopulation(int populationSize, GeneticAlgorithmParameters parameters, TestFunction testFunction)
        {
            Individual[] individuals = new Individual[populationSize];
            (double min, double max) = (testFunction.MinDomain, testFunction.MaxDomain);

            for (int i = 0; i < populationSize; i++)
            {
                double[] genes = new double[parameters.GenotypeLength];

                for (int j = 0; j < genes.Length; j++)
                {
                    genes[j] = min + RandomProvider.NextDouble() * (max - min);
                }

                individuals[i] = new Individual(genes, testFunction);
            }

            return new Population(individuals);
        }
        public static Population InitializePopulationParallel(int populationSize, GeneticAlgorithmParameters parameters, TestFunction testFunction, int threadCount)
        {
            Individual[] individuals = new Individual[populationSize];
            (double min, double max) = (testFunction.MinDomain, testFunction.MaxDomain);

            Parallel.For(0, populationSize, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, i =>
            {
                double[] genes = new double[parameters.GenotypeLength];

                for (int j = 0; j < genes.Length; j++)
                {
                    genes[j] = min + RandomProvider.NextDouble() * (max - min);
                }

                individuals[i] = new Individual(genes, testFunction);
            });

            return new Population(individuals);
        }

        public static void EvaluateFitness(Individual individual, TestFunction testFunction)
        {
            individual.Fitness = testFunction.Function(individual.Genotype);
        }
        public static void EvaluateFitness(Population population, TestFunction testFunction)
        {
            for (int i = 0; i < population.Individuals.Length; i++)
            {
                population.Individuals[i].Fitness = testFunction.Function(population.Individuals[i].Genotype);
            }
        }
        public static void EvaluateFitnessParallel(Population population, TestFunction testFunction, int threadCount)
        {
            Parallel.For(0, population.Individuals.Length, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, i =>
            {
                Individual individual = population.Individuals[i];
                individual.Fitness = testFunction.Function(individual.Genotype);
            });
        }

        public static async Task EvaluateFitnessAsync(Individual individual, TestFunction testFunction, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                individual.Fitness = testFunction.Function(individual.Genotype);
            }, cancellationToken);
        }
        public static async Task EvaluateFitnessAsync(Population population, TestFunction testFunction, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < population.Individuals.Length; i++)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    population.Individuals[i].Fitness = testFunction.Function(population.Individuals[i].Genotype);
                }
            }, cancellationToken);
        }
        public static Task EvaluateFitnessParallelAsync(Population population, TestFunction testFunction, int threads, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Parallel.For(0, population.Individuals.Length, new ParallelOptions { MaxDegreeOfParallelism = threads, CancellationToken = cancellationToken }, i =>
                {
                    population.Individuals[i].Fitness = testFunction.Function(population.Individuals[i].Genotype);
                });
            }, cancellationToken);
        }

        public static void SortByFitness(Individual[] population)
        {
            Array.Sort(population, (a, b) => a.Fitness.CompareTo(b.Fitness));
        }
        public static void SortByFitness(List<Individual> population)
        {
            population.Sort((a, b) => a.Fitness.CompareTo(b.Fitness));
        }
        public static void SortByFitness(Population population)
        {
            Array.Sort(population.Individuals, (a, b) => a.Fitness.CompareTo(b.Fitness));
        }
        
        public static FitnessStats GetFitnessSummary(Population population)
        {
            int n = population.Individuals.Length;
            double min = population.Individuals[0].Fitness;
            double max = population.Individuals[0].Fitness;
            double average = population.Individuals[0].Fitness;

            for (int i = 1; i < n; i++)
            {
                double fitness = population.Individuals[i].Fitness;
                if (min > fitness) min = fitness;
                if (max < fitness) max = fitness;
                average += fitness;
            }

            average /= n;

            return new FitnessStats(min, max, average);
        }
        
        public static int GetWorstIndex(Individual[] individuals)
        {
            int worstIndex = 0;
            double worstFitness = individuals[0].Fitness;

            for (int i = 1; i < individuals.Length; i++)
            {
                if (individuals[i].Fitness > worstFitness)
                {
                    worstFitness = individuals[i].Fitness;
                    worstIndex = i;
                }
            }

            return worstIndex;
        }
        public static (int, int) GetWorstIndexes(Individual[] individuals)
        {
            int worstIndex1 = 0;
            int worstIndex2 = 1;
            double worstFitness1 = individuals[worstIndex1].Fitness;
            double worstFitness2 = individuals[worstIndex2].Fitness;

            if (worstFitness1 < worstFitness2)
            {
                (worstIndex1, worstIndex2) = (worstIndex2, worstIndex1);
                (worstFitness1, worstFitness2) = (worstFitness2, worstFitness1);
            }

            for (int i = 2; i < individuals.Length; i++)
            {
                double fitness = individuals[i].Fitness;

                if (fitness > worstFitness1)
                {
                    worstIndex2 = worstIndex1;
                    worstFitness2 = worstFitness1;

                    worstIndex1 = i;
                    worstFitness1 = fitness;
                }
                else if (fitness > worstFitness2)
                {
                    worstIndex2 = i;
                    worstFitness2 = fitness;
                }
            }

            return (worstIndex1, worstIndex2);
        }
        public static void TryReplaceWorst(Individual[] population, Individual child1, Individual child2)
        {
            (int worstIndex1, int worstIndex2) = GetWorstIndexes(population);

            Individual worst1 = population[worstIndex1];
            Individual worst2 = population[worstIndex2];

            bool child1BetterThanWorst1 = child1.Fitness < worst1.Fitness;
            bool child1BetterThanWorst2 = child1.Fitness < worst2.Fitness;

            bool child2BetterThanWorst1 = child2.Fitness < worst1.Fitness;
            bool child2BetterThanWorst2 = child2.Fitness < worst2.Fitness;

            if (child1BetterThanWorst1 && child2BetterThanWorst2)
            {
                population[worstIndex1] = child1;
                population[worstIndex2] = child2;

                return;
            }
            if (child1BetterThanWorst2 && child2BetterThanWorst1)
            {
                population[worstIndex1] = child2;
                population[worstIndex2] = child1;

                return;
            }

            if (child1BetterThanWorst1)
            {
                population[worstIndex1] = child1;

                return;
            }
            if (child1BetterThanWorst2)
            {
                population[worstIndex2] = child1;

                return;
            }

            if (child2BetterThanWorst1)
            {
                population[worstIndex1] = child2;

                return;
            }
            if (child2BetterThanWorst2)
            {
                population[worstIndex2] = child2;

                return;
            }
        }

        public static Individual GetBest(Population population)
        {
            int n = population.Individuals.Length;
            Individual individual = population.Individuals[0];
            double min = individual.Fitness;

            for (int i = 1; i < n; i++)
            {
                double fitness = population.Individuals[i].Fitness;
                if (min > fitness)
                {
                    min = fitness;
                    individual = population.Individuals[i];
                }
            }

            return individual.Clone();
        }

        public static Individual[] GetElites(Population population, int count)
        {
            Individual[] elites = new Individual[count];
            int n = population.Individuals.Length;

            if (count >= n / 2)
            {
                SortByFitness(population);

                for (int i = 0; i < count; i++)
                {
                    elites[i] = population.Individuals[i].Clone();
                }

                return elites;
            }

            QuickSelect(population.Individuals, 0, n - 1, count);

            for (int i = 0; i < count; i++)
            {
                elites[i] = population.Individuals[i].Clone();
            }

            SortByFitness(elites);

            return elites;
        }
        private static void QuickSelect(Individual[] array, int left, int right, int k)
        {
            while (left < right)
            {
                int pivotIndex = Partition(array, left, right);
                int length = pivotIndex - left + 1;

                if (k == length)
                {
                    return;
                }
                else if (k < length)
                {
                    right = pivotIndex - 1;
                }
                else
                {
                    k -= length;
                    left = pivotIndex + 1;
                }
            }
        }
        private static int Partition(Individual[] array, int left, int right)
        {
            double pivotFitness = array[right].Fitness;
            int i = left;

            for (int j = left; j < right; j++)
            {
                if (array[j].Fitness <= pivotFitness)
                {
                    Swap(array, i, j);
                    i++;
                }
            }

            Swap(array, i, right);

            return i;
        }
        private static void Swap(Individual[] array, int i, int j)
        {
            (array[j], array[i]) = (array[i], array[j]);
        }
    }
}
