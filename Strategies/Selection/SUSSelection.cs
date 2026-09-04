using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Selection
{
    public class SUSSelection
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
            (double[] inverseFitnesses, double totalInverseFitness) = inverseFitnessesAndTotal;
            double distance = totalInverseFitness / count;
            double start = RandomProvider.NextDouble() * distance;

            double[] pointers = new double[individuals];

            for (int i = 0; i < individuals; i++)
            {
                pointers[i] = start + i * distance;
            }

            Individual[] selected = new Individual[individuals];
            int index = 0;
            double cumulative = inverseFitnesses[0];

            for (int i = 0; i < individuals; i++)
            {
                while (pointers[i] > cumulative && index < count - 1)
                {
                    index++;
                    cumulative += inverseFitnesses[index];
                }

                selected[i] = population.Individuals[index];
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

            // Oblicz odwrotności fitness i sumę
            double[] inverseFitnesses = new double[count];
            double totalInverseFitness = 0;

            for (int i = 0; i < count; i++)
            {
                inverseFitnesses[i] = 1.0 / (population[i].Fitness - fMin + 1e-10);
                totalInverseFitness += inverseFitnesses[i];
            }

            double distance = totalInverseFitness / individuals;
            double start = RandomProvider.NextDouble() * distance;

            double[] pointers = new double[individuals];
            for (int i = 0; i < individuals; i++)
            {
                pointers[i] = start + i * distance;
            }

            int index = 0;
            double cumulative = inverseFitnesses[0];

            for (int i = 0; i < individuals; i++)
            {
                while (index < count - 1 && pointers[i] > cumulative)
                {
                    index++;
                    cumulative += inverseFitnesses[index];
                }

                selected[i] = population[index];
            }

            return selected;
        }
    }
}
