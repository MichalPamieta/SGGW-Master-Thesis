using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Crossover
{
    public class OnePointCrossover
    {
        public static (Individual child1, Individual child2) Crossover(Individual parent1, Individual parent2)
        {
            int length = parent1.Genotype.Length;
            int point = RandomProvider.Next(1, length);

            Individual child1 = new(length);
            Individual child2 = new(length);

            Array.Copy(parent1.Genotype, 0, child1.Genotype, 0, point);
            Array.Copy(parent2.Genotype, 0, child2.Genotype, 0, point);

            Array.Copy(parent2.Genotype, point, child1.Genotype, point, length - point);
            Array.Copy(parent1.Genotype, point, child2.Genotype, point, length - point);

            return (child1, child2);
        }
    }
}
