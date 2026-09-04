using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Crossover;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Managers
{
    public class CrossoverManager(GeneticAlgorithmParameters parameters)
    {
        private readonly GeneticAlgorithmParameters parameters = parameters;

        public (Individual parent1, Individual parent2) ApplyCrossover(Individual parent1, Individual parent2)
        {
            if (RandomProvider.NextDouble() < parameters.CrossoverRate)
            {
                return parameters.CrossoverType switch
                {
                    CrossoverType.OnePoint => OnePointCrossover.Crossover(parent1, parent2),
                    CrossoverType.TwoPoint => TwoPointCrossover.Crossover(parent1, parent2),
                    CrossoverType.MultiPoint => MultiPointCrossover.Crossover(parent1, parent2, parameters.MultiPointCrossoverPoints),
                    CrossoverType.Uniform => UniformCrossover.Crossover(parent1, parent2),
                    _ => throw new InvalidOperationException($"Unsupported crossover type: {parameters.CrossoverType}")
                };
            }

            return (parent1, parent2);
        }
    }
}
