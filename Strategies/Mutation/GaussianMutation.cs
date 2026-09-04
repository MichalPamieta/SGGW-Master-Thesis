using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Mutation
{
    public class GaussianMutation
    {
        public static void Mutate(Individual individual, double min, double max, double prob = 0.5, double stdDev = 1.0)
        {
            int length = individual.Genotype.Length;

            for (int i = 0; i < length; i++)
            {
                if (RandomProvider.NextDouble() < prob)
                {
                    double u1 = 1.0 - RandomProvider.NextDouble();
                    double u2 = 1.0 - RandomProvider.NextDouble();
                    double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

                    double mutated = individual.Genotype[i] + randStdNormal * stdDev;
                    individual.Genotype[i] = Math.Clamp(mutated, min, max);
                }
            }
        }
    }
}
