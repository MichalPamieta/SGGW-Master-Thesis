using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Elitism
{
    public class Elitism
    {
        public static int GetEliteCount(GeneticAlgorithmParameters parameters, int populationSize, int eliteCount, double eliteFraction)
        {
            return parameters.ElitismValueType switch
            {
                ElitismValueType.Fixed => Math.Clamp(eliteCount, 1, populationSize),
                ElitismValueType.Percentage => Math.Clamp((int)Math.Ceiling(Math.Clamp(eliteFraction, 1e-10, 1.0) * populationSize), 1, populationSize),
                _ => 1
            };
        }

        public static Individual[] Select(Individual[] population, int eliteCount, bool isSorted = true, bool returnSorted = true)
        {
            int n = population.Length;
            eliteCount = Math.Clamp(eliteCount, 1, n);
            Individual[] elites = new Individual[eliteCount];
            PriorityQueue<Individual, double> heap = new();

            if (!isSorted)
            {
                PopulationHelper.SortByFitness(population);
            }

            for (int i = 0; i < n; i++)
            {
                Individual individual = population[i];
                double negFitness = -individual.Fitness;

                if (heap.Count < eliteCount)
                {
                    heap.Enqueue(individual, negFitness);
                }
                else if (heap.Peek().Fitness > individual.Fitness)
                {
                    heap.EnqueueDequeue(individual, negFitness);
                }
            }

            int index = 0;
            while (heap.Count > 0)
            {
                elites[index++] = heap.Dequeue().Clone();
            }

            if (returnSorted)
            {
                PopulationHelper.SortByFitness(elites);
            }

            return elites;
        }

        public static Population Insert(Population population, Individual[] elites)
        {
            int originalLength = population.Individuals.Length;
            int elitesLength = elites.Length;

            Individual[] newIndividuals = new Individual[originalLength + elitesLength];

            Array.Copy(population.Individuals, 0, newIndividuals, 0, originalLength);

            Array.Copy(elites, 0, newIndividuals, originalLength, elitesLength);

            population.Individuals = newIndividuals;

            return population;
        }

        public static Population Replace(Population population, Individual[] elites, int eliteCount, bool returnSorted = true, bool modifyOriginal = true)
        {
            int n = population.Individuals.Length;
            int m = elites.Length;
            eliteCount = Math.Clamp(eliteCount, 1, Math.Min(n, m));

            PriorityQueue<int, double> worstIndexesByFitness = new();
            HashSet<int> selectedIndexes = [];
            PriorityQueue<Individual, double> bestElitesByFitness = new();

            population = modifyOriginal ? population : population.Clone();

            for (int i = 0; i < n; i++)
            {
                double negFitness = -population.Individuals[i].Fitness;

                if (worstIndexesByFitness.Count < eliteCount)
                {
                    worstIndexesByFitness.Enqueue(i, negFitness);
                    selectedIndexes.Add(i);
                }
                else if (worstIndexesByFitness.TryPeek(out _, out double bestWorstFitness) && negFitness > bestWorstFitness)
                {
                    int replacedIndex = worstIndexesByFitness.EnqueueDequeue(i, negFitness);
                    selectedIndexes.Remove(replacedIndex);
                    selectedIndexes.Add(i);
                }
            }

            for (int i = 0; i < m; i++)
            {
                Individual individual = elites[i];
                double negFitness = -individual.Fitness;

                if (bestElitesByFitness.Count < eliteCount)
                {
                    bestElitesByFitness.Enqueue(individual, negFitness);
                }
                else if (bestElitesByFitness.TryPeek(out _, out double worstBestFitness) && negFitness > worstBestFitness)
                {
                    bestElitesByFitness.EnqueueDequeue(individual, negFitness);
                }
            }

            while (worstIndexesByFitness.Count > 0 && bestElitesByFitness.Count > 0)
            {
                population.Individuals[worstIndexesByFitness.Dequeue()] = bestElitesByFitness.Dequeue().Clone();
            }

            if (returnSorted)
            {
                PopulationHelper.SortByFitness(population);
            }

            return population;
        }

        public static Population Select(Population population, int eliteCount, bool isSorted = true, bool returnSorted = true)
        {
            int n = population.Individuals.Length;
            eliteCount = Math.Clamp(eliteCount, 1, n);

            Individual[] elites = new Individual[eliteCount];
            PriorityQueue<Individual, double> heap = new();

            if (!isSorted)
            {
                PopulationHelper.SortByFitness(population);
            }

            for (int i = 0; i < n; i++)
            {
                Individual individual = population.Individuals[i];
                double negFitness = -individual.Fitness;

                if (heap.Count < eliteCount)
                {
                    heap.Enqueue(individual, negFitness);
                }
                else if (heap.Peek().Fitness > individual.Fitness)
                {
                    heap.EnqueueDequeue(individual, negFitness);
                }
            }

            for (int i = eliteCount - 1; i >= 0; i--)
            {
                elites[i] = heap.Dequeue().Clone();
            }

            if (returnSorted)
            {
                PopulationHelper.SortByFitness(elites);
            }

            return new Population(elites);
        }
    }
}
