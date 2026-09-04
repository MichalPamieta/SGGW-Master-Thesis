using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Mutation;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Managers
{
    public class MutationManager(GeneticAlgorithmParameters parameters, TestFunction testFunction)
    {
        private readonly GeneticAlgorithmParameters parameters = parameters;
        private readonly TestFunction testFunction = testFunction;

        public void Mutate(Individual individual)
        {
            if (RandomProvider.NextDouble() < parameters.MutationProbability)
            {
                switch (parameters.MutationType)
                {
                    case MutationType.Random:
                        RandomMutation.Mutate(individual, testFunction.MinDomain, testFunction.MaxDomain, parameters.GeneMutationProbability);
                        break;

                    case MutationType.Gaussian:
                        GaussianMutation.Mutate(individual, testFunction.MinDomain, testFunction.MaxDomain, parameters.GeneMutationProbability, parameters.GaussianSigma);
                        break;
                }
            }
        }
    }
}
