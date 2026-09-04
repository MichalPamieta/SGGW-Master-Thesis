using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Selection
{
    public class TruncationSelection
    {
        public static Individual[] Select(Population population, double fraction, int individuals, bool isSorted = true)
        {
            if (!isSorted)
            {
                PopulationHelper.SortByFitness(population);
            }

            Individual[] selected = new Individual[individuals];
            fraction = Math.Clamp(fraction, 1e-10, 1);
            int cutoff = (int)(population.Individuals.Length * fraction);
            cutoff = Math.Clamp(cutoff, 1, population.Individuals.Length);

            for (int i = 0; i < individuals; i++)
            {
                selected[i] = population.Individuals[RandomProvider.Next(cutoff)];
            }

            return selected;
        }

        public static Individual[] Select(List<Individual> population, double fraction, int individuals, bool isSorted = false)
        {
            if (!isSorted)
            {
                PopulationHelper.SortByFitness(population);
            }

            Individual[] selected = new Individual[individuals];
            fraction = Math.Clamp(fraction, 1e-10, 1);
            int cutoff = (int)(population.Count * fraction);
            cutoff = Math.Clamp(cutoff, 1, population.Count);

            for (int i = 0; i < individuals; i++)
            {
                selected[i] = population[RandomProvider.Next(cutoff)];
            }

            return selected;
        }
    }
}
