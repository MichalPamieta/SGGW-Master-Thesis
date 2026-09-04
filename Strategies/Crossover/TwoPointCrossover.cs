using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Crossover
{
    public class TwoPointCrossover
    {
        public static (Individual child1, Individual child2) Crossover(Individual parent1, Individual parent2)
        {
            int length = parent1.Genotype.Length;
            int point1 = RandomProvider.Next(1, length - 1);
            int point2 = RandomProvider.Next(point1, length);

            Individual child1 = new(length);
            Individual child2 = new(length);

            Array.Copy(parent1.Genotype, 0, child1.Genotype, 0, point1);
            Array.Copy(parent2.Genotype, 0, child2.Genotype, 0, point1);

            Array.Copy(parent2.Genotype, point1, child1.Genotype, point1, point2 - point1);
            Array.Copy(parent1.Genotype, point1, child2.Genotype, point1, point2 - point1);

            Array.Copy(parent1.Genotype, point2, child1.Genotype, point2, length - point2);
            Array.Copy(parent2.Genotype, point2, child2.Genotype, point2, length - point2);

            return (child1, child2);
        }
    }
}
