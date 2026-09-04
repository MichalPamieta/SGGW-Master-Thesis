using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Selection
{
    public class RankLinearSelection
    {
        public static (double[] weights, double totalWeights) CalculateWeightsAndTotal(Population population, double sp, bool isSorted = true)
        {
            if (!isSorted)
            {
                PopulationHelper.SortByFitness(population);
            }

            int count = population.Individuals.Length;
            double[] weights = new double[count];
            double totalWeights = 0;
            sp = Math.Clamp(sp, 1.0, 2.0);

            for (int i = 0; i < count; i++)
            {
                weights[i] = (2.0 - sp) / count + 2.0 * (count - 1 - i) * (sp - 1) / (count * (count - 1));
                totalWeights += weights[i];
            }

            return (weights, totalWeights);
        }

        public static Individual[] Select(Population population, (double[], double) weightsAndTotal, int individuals, bool isSorted = true)
        {
            if (!isSorted)
            {
                PopulationHelper.SortByFitness(population);
            }

            int count = population.Individuals.Length;
            Individual[] selected = new Individual[individuals];
            (double[] weights, double totalWeight) = weightsAndTotal;

            for (int i = 0; i < individuals; i++)
            {
                double rank = RandomProvider.NextDouble() * totalWeight;
                double cumulative = 0;

                for (int j = 0; j < count; j++)
                {
                    cumulative += weights[j];

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
            sp = Math.Clamp(sp, 1.0, 2.0);

            for (int i = 0; i < count; i++)
            {
                weights[i] = (2.0 - sp) / count + 2.0 * (count - 1 - i) * (sp - 1) / (count * (count - 1));
                totalWeight += weights[i];
            }

            for (int i = 0; i < individuals; i++)
            {
                double rank = RandomProvider.NextDouble() * totalWeight;
                double cumulative = 0;

                for (int j = 0; j < count; j++)
                {
                    cumulative += weights[j];

                    if (rank < cumulative)
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
