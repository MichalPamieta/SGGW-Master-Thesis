using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Crossover
{
    public class MultiPointCrossover
    {
        public static (Individual child1, Individual child2) Crossover(Individual parent1, Individual parent2, int crossoverPoints)
        {
            int length = parent1.Genotype.Length;
            Individual child1 = new(length);
            Individual child2 = new(length);

            HashSet<int> pointSet = [];
            while (pointSet.Count < crossoverPoints)
            {
                int point = RandomProvider.Next(1, length - 1);
                pointSet.Add(point);
            }

            int[] points = [.. pointSet];

            Array.Sort(points);

            int[] segments = new int[crossoverPoints + 2];
            segments[0] = 0;

            for (int i = 0; i < crossoverPoints; i++)
            {
                segments[i + 1] = points[i];
            }
            segments[crossoverPoints + 1] = length;

            bool swap = false;

            for (int seg = 0; seg < segments.Length - 1; seg++)
            {
                int start = segments[seg];
                int count = segments[seg + 1] - start;

                if (!swap)
                {
                    Array.Copy(parent1.Genotype, start, child1.Genotype, start, count);
                    Array.Copy(parent2.Genotype, start, child2.Genotype, start, count);
                }
                else
                {
                    Array.Copy(parent2.Genotype, start, child1.Genotype, start, count);
                    Array.Copy(parent1.Genotype, start, child2.Genotype, start, count);
                }

                swap = !swap;
            }

            return (child1, child2);
        }
    }
}
