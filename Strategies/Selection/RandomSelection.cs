using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Selection
{
    public class RandomSelection
    {
        public static Individual[] Select(Population population, int individuals)
        {
            int count = population.Individuals.Length;
            Individual[] selected = new Individual[individuals];

            for (int i = 0; i < individuals; i++)
            {
                selected[i] = population.Individuals[RandomProvider.Next(count)];
            }

            return selected;
        }

        public static Individual[] Select(List<Individual> population, int individuals)
        {
            int count = population.Count;
            Individual[] selected = new Individual[individuals];

            for (int i = 0; i < individuals; i++)
            {
                selected[i] = population[RandomProvider.Next(count)];
            }

            return selected;
        }
    }
}
