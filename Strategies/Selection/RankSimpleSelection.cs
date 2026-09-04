using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Selection
{
    public class RankSimpleSelection
    {
        public static Individual[] Select(Population population, int individuals, bool isSorted = true)
        {
            if (!isSorted)
            {
                PopulationHelper.SortByFitness(population);
            }

            int count = population.Individuals.Length;
            Individual[] selected = new Individual[individuals];
            double totalRank = count * (count + 1) / 2.0;

            for (int i = 0; i < individuals; i++)
            {
                double rank = RandomProvider.NextDouble() * totalRank;
                double cumulative = 0;

                for (int j = 0; j < count; j++)
                {
                    cumulative += (count - j);

                    if (rank < cumulative)
                    {
                        selected[i] = population.Individuals[j];
                        break;
                    }
                }
            }

            return selected;
        }

        public static Individual[] Select(List<Individual> population, int individuals, bool isSorted = false)
        {
            if (!isSorted)
            {
                PopulationHelper.SortByFitness(population);
            }

            int count = population.Count;
            Individual[] selected = new Individual[individuals];
            double totalRank = count * (count + 1) / 2.0;

            for (int i = 0; i < individuals; i++)
            {
                double rank = RandomProvider.NextDouble() * totalRank;
                double cumulative = 0;

                for (int j = 0; j < count; j++)
                {
                    cumulative += (count - j);

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
