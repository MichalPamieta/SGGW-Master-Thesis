using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Mutation
{
    public class RandomMutation
    {
        public static void Mutate(Individual individual, double min, double max, double prob = 0.5)
        {
            int length = individual.Genotype.Length;

            for (int i = 0; i < length; i++)
            {
                if (RandomProvider.NextDouble() < prob)
                {
                    individual.Genotype[i] = RandomProvider.NextDouble() * (max - min) + min;
                }
            }
        }
    }
}
