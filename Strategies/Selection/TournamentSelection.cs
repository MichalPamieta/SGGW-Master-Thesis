using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Selection
{
    public class TournamentSelection
    {
        public static Individual[] Select(Population population, int tournamentSize, int individuals)
        {
            int count = population.Individuals.Length;
            Individual[] selected = new Individual[individuals];

            for (int i = 0; i < individuals; i++)
            {
                Individual best = population.Individuals[RandomProvider.Next(count)];

                for (int j = 1; j < tournamentSize; j++)
                {
                    Individual candidate = population.Individuals[RandomProvider.Next(count)];

                    if (best.Fitness > candidate.Fitness)
                    {
                        best = candidate;
                    }
                }

                selected[i] = best;
            }

            return selected;
        }

        public static Individual[] Select(List<Individual> population, int tournamentSize, int individuals)
        {
            int count = population.Count;
            Individual[] selected = new Individual[individuals];

            for (int i = 0; i < individuals; i++)
            {
                Individual best = population[RandomProvider.Next(count)];

                for (int j = 1; j < tournamentSize; j++)
                {
                    Individual candidate = population[RandomProvider.Next(count)];

                    if (best.Fitness > candidate.Fitness)
                    {
                        best = candidate;
                    }
                }

                selected[i] = best;
            }

            return selected;
        }
    }
}
