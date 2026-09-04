using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Elitism;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Managers
{
    public class ElitismManager(GeneticAlgorithmParameters parameters)
    {
        private int eliteCount = 1;
        public int EliteCount => eliteCount;

        private readonly GeneticAlgorithmParameters parameters = parameters;

        public void Preprocess(int populationSize)
        {
            eliteCount = Elitism.GetEliteCount(parameters, populationSize, parameters.EliteCount, parameters.ElitePercentage);
        }

        public Individual[] SelectElites(Population population, bool isSorted = true, bool returnSorted = true)
        {
            return Elitism.Select(population.Individuals, eliteCount, isSorted, returnSorted);
        }

        public Population ApplyElitism(Population population, Individual[] elites, bool modifyOriginal = true, bool returnSorted = true)
        {
            return parameters.ElitismType switch
            {
                ElitismType.Insertion => Elitism.Insert(population, elites),
                ElitismType.Replacement => Elitism.Replace(population, elites, eliteCount, modifyOriginal, returnSorted),
                _ => throw new InvalidOperationException($"Unsupported elitism type: {parameters.ElitismType}")
            };
        }
    }
}
