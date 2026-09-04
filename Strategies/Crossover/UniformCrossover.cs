using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Crossover
{
    public class UniformCrossover
    {
        public static (Individual child1, Individual child2) Crossover(Individual parent1, Individual parent2)
        {
            int length = parent1.Genotype.Length;
            Individual child1 = new(length);
            Individual child2 = new(length);

            for (int i = 0; i < length; i++)
            {
                if (RandomProvider.NextDouble() < 0.5)
                {
                    child1.Genotype[i] = parent1.Genotype[i];
                    child2.Genotype[i] = parent2.Genotype[i];
                }
                else
                {
                    child1.Genotype[i] = parent2.Genotype[i];
                    child2.Genotype[i] = parent1.Genotype[i];
                }
            }

            return (child1, child2);
        }
    }
}
