using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Selection
{
    public class RankExponentialSelection
    {
        public static (double[] weights, double totalWeight) CalculateWeightsAndTotal(Population population, double sp, bool isSorted = true)
        {
            if (!isSorted)
            {
                PopulationHelper.SortByFitness(population);
            }

            int count = population.Individuals.Length;
            double[] weights = new double[count];
            double totalWeight = 0;
            sp = Math.Max(1e-10, sp);
            double norm = (1 - Math.Exp(-sp)) / (1 - Math.Exp(-sp * count));

            for (int i = 0; i < count; i++)
            {
                weights[i] = norm * Math.Exp(-sp * i);
                totalWeight += weights[i];
            }

            return (weights, totalWeight);
        }

        public static Individual[] Select(Population population, (double[], double) weightsWithTotal, int individuals, bool isSorted = true)
        {
            if (!isSorted)
            {
                PopulationHelper.SortByFitness(population);
            }

            int count = population.Individuals.Length;
            Individual[] selected = new Individual[individuals];
            (double[] cumulativeWeights, double totalWeight) = weightsWithTotal;

            for (int i = 0; i < individuals; i++)
            {
                double rank = RandomProvider.NextDouble() * totalWeight;
                double cumulative = 0;

                for (int j = 0; j < count; j++)
                {
                    cumulative += cumulativeWeights[j];

                    if (rank < cumulative)
                    {
                        selected[i] = population.Individuals[j];
                        break;
                    }
                }
            }

            return selected;
        }

        public static Individual[] Select(List<Individual> population, double sp, int individuals, bool isSorted = false)
        {
            if (!isSorted)
            {
                PopulationHelper.SortByFitness(population);
            }

            Individual[] selected = new Individual[individuals];
            int count = population.Count;

            double[] weights = new double[count];
            double totalWeight = 0;
            sp = Math.Max(1e-10, sp);
            double norm = (1 - Math.Exp(-sp)) / (1 - Math.Exp(-sp * count));

            for (int i = 0; i < count; i++)
            {
                weights[i] = norm * Math.Exp(-sp * i);
                totalWeight += weights[i];
            }

            for (int i = 0; i < individuals; i++)
            {
                double threshold = RandomProvider.NextDouble() * totalWeight;
                double cumulative = 0;

                for (int j = 0; j < count; j++)
                {
                    cumulative += weights[j];
                    if (threshold < cumulative)
                    {
                        selected[i] = population[j];
                        break;
                    }
                }
            }

            return selected;
        }
    }
}
