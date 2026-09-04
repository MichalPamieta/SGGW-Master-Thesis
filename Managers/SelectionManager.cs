using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Strategies.Selection;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Managers
{
    public class SelectionManager(GeneticAlgorithmParameters parameters)
    {
        private readonly GeneticAlgorithmParameters parameters = parameters;

        private double[] weights = new double[parameters.PopulationSize];
        private double totalWeight = double.MaxValue;

        public void Preprocess(Population population)
        {
            bool needsSorting = parameters.SelectionType == SelectionType.Truncation ||
                parameters.SelectionType == SelectionType.RankSimple ||
                parameters.SelectionType == SelectionType.RankLinear ||
                parameters.SelectionType == SelectionType.RankExponential;

            if (needsSorting)
            {
                PopulationHelper.SortByFitness(population);
            }

            switch (parameters.SelectionType)
            {
                case SelectionType.RankLinear:
                    (weights, totalWeight) = RankLinearSelection.CalculateWeightsAndTotal(population, parameters.SelectionPressureLinear);
                    break;

                case SelectionType.RankExponential:
                    (weights, totalWeight) = RankExponentialSelection.CalculateWeightsAndTotal(population, parameters.SelectionPressureExponential);
                    break;

                case SelectionType.Roulette:
                    (weights, totalWeight) = RouletteSelection.CalculateInverseFitnessesAndTotal(population);
                    break;

                case SelectionType.SUS:
                    (weights, totalWeight) = SUSSelection.CalculateInverseFitnessesAndTotal(population);
                    break;

                default:
                    break;
            }
        }

        public (Individual Parent1, Individual Parent2) SelectParents(Population population)
        {
            Individual[] parents = parameters.SelectionType switch
            {
                SelectionType.Random => RandomSelection.Select(population, 2),
                SelectionType.Tournament => TournamentSelection.Select(population, parameters.TournamentSize, 2),
                SelectionType.Roulette => RouletteSelection.Select(population, (weights, totalWeight), 2),
                SelectionType.SUS => SUSSelection.Select(population, (weights, totalWeight), 2),
                SelectionType.RankSimple => RankSimpleSelection.Select(population, 2),
                SelectionType.RankLinear => RankLinearSelection.Select(population, (weights, totalWeight), 2),
                SelectionType.RankExponential => RankExponentialSelection.Select(population, (weights, totalWeight), 2),
                SelectionType.Truncation => TruncationSelection.Select(population, parameters.TruncationFraction, 2),
                _ => throw new InvalidOperationException($"Unsupported selection type: {parameters.SelectionType}")
            };

            return (parents[0], parents[1]);
        }

        public (Individual Parent1, Individual Parent2) SelectParents(List<Individual> population)
        {
            Individual[] parents = parameters.SelectionType switch
            {
                SelectionType.Random => RandomSelection.Select(population, 2),
                SelectionType.Tournament => TournamentSelection.Select(population, parameters.TournamentSize, 2),
                SelectionType.Roulette => RouletteSelection.Select(population, 2),
                SelectionType.SUS => SUSSelection.Select(population, 2),
                SelectionType.RankSimple => RankSimpleSelection.Select(population, 2),
                SelectionType.RankLinear => RankLinearSelection.Select(population, parameters.SelectionPressureLinear, 2),
                SelectionType.RankExponential => RankExponentialSelection.Select(population, parameters.SelectionPressureExponential, 2),
                SelectionType.Truncation => TruncationSelection.Select(population, parameters.TruncationFraction, 2),
                _ => throw new InvalidOperationException($"Unsupported selection type: {parameters.SelectionType}")
            };

            return (parents[0], parents[1]);
        }
    }
}
