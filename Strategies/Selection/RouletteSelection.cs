using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Selection
{
    public class RouletteSelection
    {
        public static (double[] inverseFitnesses, double totalInverseFitness) CalculateInverseFitnessesAndTotal(Population population)
        {
            int count = population.Individuals.Length;
            double[] inverseFitnesses = new double[count];
            double totalInverseFitness = 0;

            double fMin = double.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                if (population.Individuals[i].Fitness < fMin)
                {
                    fMin = population.Individuals[i].Fitness;
                }
            }

            for (int i = 0; i < count; i++)
            {
                inverseFitnesses[i] = 1.0 / (population.Individuals[i].Fitness - fMin + 1e-10);
                totalInverseFitness += inverseFitnesses[i];
            }

            return (inverseFitnesses, totalInverseFitness);
        }

        public static Individual[] Select(Population population, (double[], double) inverseFitnessesAndTotal, int individuals)
        {
            int count = population.Individuals.Length;
            Individual[] selected = new Individual[individuals];
            (double[] inverseFitnesses, double totalInverseFitness) = inverseFitnessesAndTotal;

            for (int i = 0; i < individuals; i++)
            {
                double pick = RandomProvider.NextDouble() * totalInverseFitness;
                double current = 0;

                for (int j = 0; j < count; j++)
                {
                    current += inverseFitnesses[j];

                    if (current >= pick)
                    {
                        selected[i] = population.Individuals[j];
                        break;
                    }
                }
            }

            return selected;
        }

        public static Individual[] Select(List<Individual> population, int individuals)
        {
            int count = population.Count;
            Individual[] selected = new Individual[individuals];

            double fMin = double.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                if (population[i].Fitness < fMin)
                {
                    fMin = population[i].Fitness;
                }
            }

            double[] inverseFitnesses = new double[count];
            double totalInverseFitness = 0;

            for (int i = 0; i < count; i++)
            {
                inverseFitnesses[i] = 1.0 / (population[i].Fitness - fMin + 1e-10);
                totalInverseFitness += inverseFitnesses[i];
            }

            for (int i = 0; i < individuals; i++)
            {
                double pick = RandomProvider.NextDouble() * totalInverseFitness;
                double current = 0;

                for (int j = 0; j < count; j++)
                {
                    current += inverseFitnesses[j];

                    if (current >= pick)
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
