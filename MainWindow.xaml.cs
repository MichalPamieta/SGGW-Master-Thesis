using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.WPF;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Algorithms;
using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;
using SkiaSharp;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA
{
    internal class TopologyEntry
    {
        public int Source { get; set; }
        public string Targets { get; set; } = "";
    }
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cts = new();
        private readonly PropertyInfo[] _cachedProperties = typeof(GeneticAlgorithmParameters).GetProperties();
        private readonly Dictionary<string, StackPanel> _parameterPanels = [];
        private readonly List<TopologyEntry> _topologyEntries = [];
        private readonly StringBuilder _sb = new();

        public MainWindow()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            InitializeComponent();
            GenerateParameterInputs();
        }

        private void GenerateParameterInputs()
        {
            ParametersPanel.Children.Clear();
            _parameterPanels.Clear();

            GeneticAlgorithmParameters defaultParams = new();
            HashSet<string> refreshProperties =
            [
                nameof(GeneticAlgorithmParameters.ExecutionModel),
                nameof(GeneticAlgorithmParameters.ElitismValueType),
                nameof(GeneticAlgorithmParameters.SelectionType),
                nameof(GeneticAlgorithmParameters.CrossoverType),
                nameof(GeneticAlgorithmParameters.MutationType),
                nameof(GeneticAlgorithmParameters.MigrationType),
                nameof(GeneticAlgorithmParameters.MigrationTopologyType),
                nameof(GeneticAlgorithmParameters.UseElitism),
                nameof(GeneticAlgorithmParameters.StagnantLimit),
                nameof(GeneticAlgorithmParameters.PrecisionLimit),
            ];

            foreach (var prop in _cachedProperties)
            {
                var label = new TextBlock
                {
                    Text = prop.Name,
                    Margin = new Thickness(0, 4, 8, 2),
                    Width = 160
                };

                object? defaultValue = prop.GetValue(defaultParams);
                Control? control = CreateControl(prop, defaultValue);
                if (control == null)
                {
                    continue;
                }

                StackPanel panel = new()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 2, 0, 2),
                    Visibility = Visibility.Collapsed,
                    Tag = prop.Name
                };

                panel.Children.Add(label);
                panel.Children.Add(control);

                ParametersPanel.Children.Add(panel);
                _parameterPanels[prop.Name] = panel;

                if (refreshProperties.Contains(prop.Name))
                {
                    switch (control)
                    {
                        case ComboBox comboBox:
                            comboBox.SelectionChanged += (s, e) => UpdateParameterVisibility();
                            break;
                        case CheckBox checkBox:
                            checkBox.Checked += (s, e) => UpdateParameterVisibility();
                            checkBox.Unchecked += (s, e) => UpdateParameterVisibility();
                            break;
                    }
                }

                UpdateParameterVisibility();
            }
        }

        private void UpdateParameterVisibility()
        {
            foreach ((string propName, StackPanel panel) in _parameterPanels)
            {
                panel.Visibility = ShouldDisplayParameter(propName) ? Visibility.Visible : Visibility.Collapsed;
            }

            var executionModel = GetVisibleEnumValue<ExecutionModel>("ExecutionModel");

            DrawIslandFitnessChart.Visibility = IsIslandModel(executionModel) ? Visibility.Visible : Visibility.Collapsed;
            DrawIslandTopology.Visibility = IsIslandModel(executionModel) ? Visibility.Visible : Visibility.Collapsed;
        }

        private static bool IsIslandModel(ExecutionModel? model)
        {
            return model is ExecutionModel.Island or ExecutionModel.ParallelIsland or ExecutionModel.ParallelIslandFull;
        }

        private bool ShouldDisplayParameter(string propName)
        {
            ExecutionModel? executionModel = GetVisibleEnumValue<ExecutionModel>("ExecutionModel");

            if (executionModel == null)
            {
                return new HashSet<string>
                {
                    "MaxGenerations", "PopulationSize", "GenotypeLength", "RandomSeed", "StagnantLimit", "PrecisionLimit", "TestFunctionType", "ExecutionModel", "SelectionType", "CrossoverType", "MutationType"
                }.Contains(propName);
            }
            
            switch (propName)
            {
                case "MaxStagnantGenerations":
                    return IsVisibleAndChecked("StagnantLimit");

                case "PrecisionThreshold":
                    return IsVisibleAndChecked("PrecisionLimit");

                case "UseElitism":
                    return executionModel is ExecutionModel.Sequential
                                          or ExecutionModel.PartialParallel
                                          or ExecutionModel.FullyParallel
                                          or ExecutionModel.Island
                                          or ExecutionModel.ParallelIsland
                                          or ExecutionModel.ParallelIslandFull;

                case "ElitismType":
                case "ElitismValueType":
                    return IsVisibleAndChecked("UseElitism");

                case "EliteCount":
                    return IsVisibleAndChecked("UseElitism") && GetVisibleEnumValue<ElitismValueType>("ElitismValueType") is ElitismValueType.Fixed;

                case "ElitePercentage":
                    return IsVisibleAndChecked("UseElitism") && GetVisibleEnumValue<ElitismValueType>("ElitismValueType") is ElitismValueType.Percentage;

                case "TournamentSize":
                    return GetVisibleEnumValue<SelectionType>("SelectionType") is SelectionType.Tournament;

                case "SelectionPressureLinear":
                    return GetVisibleEnumValue<SelectionType>("SelectionType") is SelectionType.RankLinear;

                case "SelectionPressureExponential":
                    return GetVisibleEnumValue<SelectionType>("SelectionType") is SelectionType.RankExponential;

                case "TruncationFraction":
                    return GetVisibleEnumValue<SelectionType>("SelectionType") is SelectionType.Truncation;

                case "MultiPointCrossoverPoints":
                    return GetVisibleEnumValue<CrossoverType>("CrossoverType") is CrossoverType.MultiPoint;

                case "GaussianSigma":
                    return GetVisibleEnumValue<MutationType>("MutationType") is MutationType.Gaussian;

                case "ThreadCount":
                    return executionModel.Value.ToString().Contains("Parallel");

                case "BatchSize":
                    return executionModel is ExecutionModel.SteadyStateAsync or ExecutionModel.ParallelSteadyStateAsync;

                case "ProducerRatio":
                    return executionModel is ExecutionModel.ParallelSteadyStateAsync;

                case "IslandCount":
                case "MigrationType":
                    return IsIslandModel(executionModel);

                case "MigrationTopologyType":
                case "MigrationRate":
                case "MigrationFrequency":
                    return GetVisibleEnumValue<MigrationType>("MigrationType") is MigrationType and not MigrationType.None;

                case "StarCenterId":
                    return GetVisibleEnumValue<MigrationTopologyType>("MigrationTopologyType") is MigrationTopologyType.Star;

                case "Offset":
                case "OffsetCount":
                    return GetVisibleEnumValue<MigrationTopologyType>("MigrationTopologyType") is MigrationTopologyType.NToN;

                case "NeighborhoodType":
                    return executionModel is ExecutionModel.Cellular2D or ExecutionModel.ParallelCellular2D
                                          or ExecutionModel.Cellular3D or ExecutionModel.ParallelCellular3D
                                          or ExecutionModel.Diffusion2D or ExecutionModel.ParallelDiffusion2D
                                          or ExecutionModel.Diffusion3D or ExecutionModel.ParallelDiffusion3D;

                case "NeighborhoodRadius":
                case "WrapNeighborhood":
                case "CenterAlwaysParent":
                case "ReplaceOnlyIfBetter":
                    return new[]
                    {
                        ExecutionModel.Cellular1D, ExecutionModel.ParallelCellular1D,
                        ExecutionModel.Cellular2D, ExecutionModel.ParallelCellular2D,
                        ExecutionModel.Cellular3D, ExecutionModel.ParallelCellular3D,
                        ExecutionModel.Diffusion1D, ExecutionModel.ParallelDiffusion1D,
                        ExecutionModel.Diffusion2D, ExecutionModel.ParallelDiffusion2D,
                        ExecutionModel.Diffusion3D, ExecutionModel.ParallelDiffusion3D
                    }.Contains(executionModel.Value);
                
                case "GeneticDriftFrequency":
                case "GeneticDriftProbability":
                    return new[]
                    {
                        ExecutionModel.Diffusion1D, ExecutionModel.ParallelDiffusion1D,
                        ExecutionModel.Diffusion2D, ExecutionModel.ParallelDiffusion2D,
                        ExecutionModel.Diffusion3D, ExecutionModel.ParallelDiffusion3D
                    }.Contains(executionModel.Value);
            }

            HashSet<string> commonParams = ["MaxGenerations", "PopulationSize", "GenotypeLength", "RandomSeed", "StagnantLimit", "PrecisionLimit", "TestFunctionType", "ExecutionModel", "SelectionType", "CrossoverType", "CrossoverRate", "MutationType", "MutationProbability", "GeneMutationProbability"];

            return commonParams.Contains(propName);
        }

        private TEnum? GetVisibleEnumValue<TEnum>(string propName) where TEnum : struct, Enum
        {
            if (_parameterPanels.TryGetValue(propName, out var panel) && panel.Visibility == Visibility.Visible && panel.Children[1] is ComboBox cb && cb.SelectedItem is TEnum selected)
            {
                return selected;
            }

            return null;
        }

        private bool IsVisibleAndChecked(string propName)
        {
            if (_parameterPanels.TryGetValue(propName, out var panel) && panel.Visibility == Visibility.Visible && panel.Children[1] is CheckBox cb)
            {
                return cb.IsChecked == true;
            }

            return false;
        }

        private static Control? CreateControl(PropertyInfo prop, object? defaultValue)
        {
            if (prop.PropertyType == typeof(string))
            {
                return new TextBox { Width = 160, Text = defaultValue?.ToString() ?? "" };
            }

            if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(double) || Nullable.GetUnderlyingType(prop.PropertyType) != null)
            {
                return new TextBox { Width = 160, Text = defaultValue?.ToString() ?? "" };
            }

            if (prop.PropertyType == typeof(bool))
            {
                return new CheckBox { IsChecked = (defaultValue as bool?) ?? false };
            }

            if (prop.PropertyType.IsEnum)
            {
                return new ComboBox
                {
                    Width = 160,
                    ItemsSource = Enum.GetValues(prop.PropertyType),
                    SelectedItem = defaultValue
                };
            }

            return null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _cts = new CancellationTokenSource();
                ProgressBar.Value = 0;
                StartButton.IsEnabled = false;
                CancelButton.IsEnabled = true;
                TopologyDataGrid.ItemsSource = null;

                var parameters = new GeneticAlgorithmParameters();

                if (!TryReadParameters(parameters))
                {
                    return;
                }

                bool isIsland = IsIslandModel(parameters.ExecutionModel);
                IslandFitnessTab.Visibility = isIsland ? Visibility.Visible : Visibility.Collapsed;
                MigrationTopologyTab.Visibility = isIsland ? Visibility.Visible : Visibility.Collapsed;

                var progress = new Progress<double>(value => ProgressBar.Value = value);

                var topologyProgress = new Progress<MigrationTopologyReport>(UpdateTopologyUI);

                IGeneticAlgorithm algorithm = CreateAlgorithmInstance(parameters.ExecutionModel);

                var result = await Task.Run(() => algorithm.Run(parameters, progress, topologyProgress, _cts.Token));

                DisplayResult(result);
            }
            finally
            {
                StartButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
            }
        }

        private bool TryReadParameters(GeneticAlgorithmParameters parameters)
        {
            foreach (var prop in _cachedProperties)
            {
                if (!_parameterPanels.TryGetValue(prop.Name, out var panel) || panel.Visibility != Visibility.Visible)
                {
                    continue;
                }

                Control control = (Control)panel.Children[1];

                if (control == null)
                {
                    continue;
                }

                try
                {
                    object value = null;
                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    switch (control)
                    {
                        case TextBox tb:
                            string text = tb.Text.Trim().Replace(',', '.');

                            if (string.IsNullOrWhiteSpace(text))
                            {
                                continue;
                            }

                            if (targetType == typeof(int))
                            {
                                if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intVal))
                                {
                                    ShowValidationError(prop.Name, "Expected an integer number (whole number without decimals).");

                                    return false;
                                }
                                if (!ValidateIntRange(prop.Name, intVal, parameters))
                                {
                                    return false;
                                }
                                value = intVal;
                            }
                            else if (targetType == typeof(double))
                            {
                                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double dblVal))
                                {
                                    ShowValidationError(prop.Name, "Expected a decimal number (use a dot as the decimal separator).");

                                    return false;
                                }
                                if (!ValidateDoubleRange(prop.Name, dblVal))
                                {
                                    return false;
                                }
                                value = dblVal;
                            }
                            else
                            {
                                value = Convert.ChangeType(text, targetType, CultureInfo.InvariantCulture);
                            }
                            break;

                        case CheckBox cb:
                            value = cb.IsChecked ?? false;
                            break;

                        case ComboBox combo when combo.SelectedItem != null:
                            value = combo.SelectedItem;
                            break;

                        default:
                            continue;

                    }

                    prop.SetValue(parameters, value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Invalid value for {prop.Name}: {ex.Message}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateIntRange(string paramName, int value, GeneticAlgorithmParameters parameters)
        {
            switch (paramName)
            {
                case "MaxGenerations":
                case "PopulationSize":
                case "GenotypeLength":
                case "TournamentSize":
                case "ThreadCount":
                case "BatchSize":
                case "IslandCount":
                case "NeighborhoodRadius":
                case "GeneticDriftFrequency":
                    if (value <= 0)
                    {
                        ShowValidationError(paramName, "Expected a positive integer (greater than 0).");

                        return false;
                    }
                    break;

                case "MaxStagnantGenerations":
                case "MigrationFrequency":
                    if (value <= 0)
                    {
                        ShowValidationError(paramName, "Expected a positive integer (greater than 0).");

                        return false;
                    }
                    if (parameters.MaxGenerations > 0 && value > parameters.MaxGenerations)
                    {
                        ShowValidationError(paramName, $"Cannot be greater than MaxGenerations.\n(Hint: This value defines an interval within total generations. Current MaxGenerations = {parameters.MaxGenerations})");

                        return false;
                    }
                    break;

                case "EliteCount":
                    if (value <= 0)
                    {
                        ShowValidationError(paramName, "Expected a positive integer (greater than 0).");

                        return false;
                    }
                    if (parameters.PopulationSize > 0 && value >= parameters.PopulationSize)
                    {
                        ShowValidationError(paramName, $"Must be less than PopulationSize.\n(Hint: Elites are selected from within the population. Current PopulationSize = {parameters.PopulationSize})");

                        return false;
                    }
                    break;

                case "MultiPointCrossoverPoints":
                    if (value <= 0)
                    {
                        ShowValidationError(paramName, "Expected a positive integer (greater than 0).");

                        return false;
                    }
                    if (parameters.GenotypeLength > 0 && value >= parameters.GenotypeLength)
                    {
                        ShowValidationError(paramName, $"Must be less than GenotypeLength.\n(Hint: Crossover points split the genotype, so must be fewer than its length. Current GenotypeLength = {parameters.GenotypeLength})");

                        return false;
                    }
                    break;

                case "StarCenterId":
                case "Offset":
                case "OffsetCount":
                    if (value < 0)
                    {
                        ShowValidationError(paramName, "Expected a non-negative integer (0 or greater).");

                        return false;
                    }
                    if (parameters.IslandCount > 0 && value >= parameters.IslandCount)
                    {
                        ShowValidationError(paramName, $"Must be less than IslandCount.\n(Hint: Islands are indexed from 0 to {parameters.IslandCount - 1})");

                        return false;
                    }
                    break;
            }

            return true;
        }

        private static bool ValidateDoubleRange(string paramName, double value)
        {
            switch (paramName)
            {
                case "PrecisionThreshold":
                case "GaussianSigma":
                    if (value <= 0.0)
                    {
                        ShowValidationError(paramName, "Expected a positive number (greater than 0).");

                        return false;
                    }
                    break;

                case "ElitePercentage":
                case "TruncationFraction":
                case "CrossoverRate":
                case "MutationProbability":
                case "GeneMutationProbability":
                case "ProducerRatio":
                case "MigrationRate":
                case "GeneticDriftProbability":
                    if (value < 0.0 || value > 1.0)
                    {
                        ShowValidationError(paramName, "Expected a value between 0.0 and 1.0.");

                        return false;
                    }
                    break;

                case "SelectionPressureLinear":
                    if (value < 1.0 || value > 2.0)
                    {
                        ShowValidationError(paramName, "Expected a value between 1.0 and 2.0.");

                        return false;
                    }
                    break;

                case "SelectionPressureExponential":
                    if (value < 0.0 || value > 5.0)
                    {
                        ShowValidationError(paramName, "Expected a value between 0.0 and 5.0.");

                        return false;
                    }
                    break;
            }

            return true;
        }

        private static void ShowValidationError(string paramName, string expectedType)
        {
            MessageBox.Show($"Invalid value for {paramName}.\n{expectedType}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void UpdateTopologyUI(MigrationTopologyReport report)
        {
            if (DrawIslandTopology?.IsChecked != true || report.MigrationTopology == null || report.MigrationTopology.Count == 0)
            {
                TopologyNameText.Text = "Topology Name: No topology defined.";
                TopologyDataGrid.ItemsSource = null;
                _topologyEntries.Clear();
                return;
            }

            TopologyNameText.Text = $"Topology Name: {report.TopologyName ?? "Unknown"}";
            _topologyEntries.Clear();

            var keys = new List<int>(report.MigrationTopology.Keys);
            keys.Sort();

            foreach (var key in keys)
            {
                var targets = report.MigrationTopology[key];
                _sb.Clear();

                for (int i = 0; i < targets.Length; i++)
                {
                    _sb.Append(targets[i]);
                    if (i < targets.Length - 1) _sb.Append(", ");
                }

                _topologyEntries.Add(new TopologyEntry
                {
                    Source = key,
                    Targets = _sb.ToString()
                });
            }

            TopologyDataGrid.ItemsSource = _topologyEntries;
        }

        private static IGeneticAlgorithm CreateAlgorithmInstance(ExecutionModel model)
        {
            return model switch
            {
                ExecutionModel.Sequential => new SequentialGA(),
                ExecutionModel.PartialParallel => new PartialParallelGA(),
                ExecutionModel.FullyParallel => new FullyParallelGA(),
                ExecutionModel.Island => new IslandGA(),
                ExecutionModel.ParallelIsland => new ParallelIslandGA(),
                ExecutionModel.ParallelIslandFull => new ParallelIslandFullGA(),
                ExecutionModel.Cellular1D => new CellularGA1D(),
                ExecutionModel.ParallelCellular1D => new ParallelCellularGA1D(),
                ExecutionModel.Cellular2D => new CellularGA2D(),
                ExecutionModel.ParallelCellular2D => new ParallelCellularGA2D(),
                ExecutionModel.Cellular3D => new CellularGA3D(),
                ExecutionModel.ParallelCellular3D => new ParallelCellularGA3D(),
                ExecutionModel.Diffusion1D => new DiffusionGA1D(),
                ExecutionModel.ParallelDiffusion1D => new ParallelDiffusionGA1D(),
                ExecutionModel.Diffusion2D => new DiffusionGA2D(),
                ExecutionModel.ParallelDiffusion2D => new ParallelDiffusionGA2D(),
                ExecutionModel.Diffusion3D => new DiffusionGA3D(),
                ExecutionModel.ParallelDiffusion3D => new ParallelDiffusionGA3D(),
                ExecutionModel.SteadyState => new SteadyStateGA(),
                ExecutionModel.SteadyStateAsync => new SteadyStateAsyncGA(),
                ExecutionModel.ParallelSteadyStateAsync => new ParallelSteadyStateAsyncGA(),
                _ => throw new NotSupportedException($"Unsupported execution model: {model}")
            };
        }
        
        private async void DisplayResult(GeneticAlgorithmResult result)
        {
            UpdateSummaryTexts(result);

            if (DrawFitnessChart.IsChecked == true)
            {
                DrawGlobalFitnessChart(result);
            }

            IslandChartsPanel.Children.Clear();

            if (DrawIslandFitnessChart.IsChecked == true)
            {
                await DrawIslandChartsAsync(result);
            }
        }

        private void UpdateSummaryTexts(GeneticAlgorithmResult result)
        {
            BestFitnessText.Text = $"Best Fitness: {result.BestIndividual.Fitness:F4}";
            GenotypeText.Text = string.Join(", ", result.BestIndividual.Genotype.Select(g => g.ToString("F2")));
            TotalTimeText.Text = $"Total Time: {result.TotalTime}";
            BestTimeText.Text = $"Time to Best: {result.BestTime}";
            BestGenerationText.Text = $"Best Generation: {result.BestGeneration}";
            GenotypeExpander.Visibility = string.IsNullOrWhiteSpace(GenotypeText.Text) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void DrawGlobalFitnessChart(GeneticAlgorithmResult result)
        {
            if (result.FitnessHistory is not { Length: > 0 })
            {
                return;
            }

            int length = result.FitnessHistory.Length;
            double[] maxStats = new double[length];
            double[] avgStats = new double[length];
            double[] minStats = new double[length];

            for (int i = 0; i < length; i++)
            {
                var stats = result.FitnessHistory[i];
                maxStats[i] = stats.Max;
                avgStats[i] = stats.Avg;
                minStats[i] = stats.Min;
            }

            FitnessChart.Series =
            [
                new LineSeries<double> { Name = "Max", Values = maxStats, GeometrySize = 0 },
                new LineSeries<double> { Name = "Avg", Values = avgStats, GeometrySize = 0 },
                new LineSeries<double> { Name = "Min", Values = minStats, GeometrySize = 0 },
            ];

            FitnessChart.XAxes = [new Axis { Name = "Generation", MinLimit = 0, MaxLimit = length, Labeler = value => $"Gen {value:F0}" }];
            FitnessChart.YAxes = [new Axis { Name = "Fitness" }];
            FitnessChart.Title = new LabelVisual { Text = "Global Fitness History", TextSize = 18, HorizontalAlignment = Align.Middle, VerticalAlignment = Align.Start, Padding = new Padding(10),
                Paint = new SolidColorPaint { Color = SKColors.Black, SKTypeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold) } };
            FitnessChart.ZoomMode = ZoomAndPanMode.X;
            FitnessChart.TooltipPosition = TooltipPosition.Top;
        }

        private async Task DrawIslandChartsAsync(GeneticAlgorithmResult result)
        {
            if (result.IslandResults is not { Length: > 0 })
            {
                return;
            }

            var islandsData = await Task.Run(() => result.IslandResults.Select(island =>
            {
                int length = island.FitnessHistory.Length;
                double[] maxStats = new double[length];
                double[] avgStats = new double[length];
                double[] minStats = new double[length];

                for (int i = 0; i < length; i++)
                {
                    var stats = island.FitnessHistory[i];
                    maxStats[i] = stats.Max;
                    avgStats[i] = stats.Avg;
                    minStats[i] = stats.Min;
                }

                return new
                {
                    island.IslandId,
                    MaxValues = maxStats,
                    AvgValues = avgStats,
                    MinValues = minStats,
                };
            }).ToList());

            foreach (var data in islandsData)
            {
                var chart = new CartesianChart
                {
                    Height = 600,
                    Margin = new Thickness(5),
                    Series =
                    [
                        new LineSeries<double> { Name = $"Island {data.IslandId} Max", Values = data.MaxValues, GeometrySize = 0 },
                        new LineSeries<double> { Name = $"Island {data.IslandId} Avg", Values = data.AvgValues, GeometrySize = 0 },
                        new LineSeries<double> { Name = $"Island {data.IslandId} Min", Values = data.MinValues, GeometrySize = 0 },
                    ],
                    XAxes = [new Axis { Name = "Generation", MinLimit = 0, MaxLimit = data.MinValues.Length, Labeler = value => $"Gen {value:F0}" }],
                    YAxes = [new Axis { Name = "Fitness" }],
                    Title = new LabelVisual
                    {
                        Text = $"Island {data.IslandId} Fitness History",
                        TextSize = 18,
                        HorizontalAlignment = Align.Middle,
                        VerticalAlignment = Align.Start,
                        Padding = new Padding(5),
                        Paint = new SolidColorPaint { Color = SKColors.Black, SKTypeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold) }
                    },
                    ZoomMode = ZoomAndPanMode.X,
                    TooltipPosition = TooltipPosition.Top,
                    AnimationsSpeed = TimeSpan.Zero
                };

                var expander = new Expander
                {
                    Header = $"Island {data.IslandId} Fitness Chart",
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    IsExpanded = true,
                    Margin = new Thickness(5),
                    Content = new Border
                    {
                        Margin = new Thickness(5),
                        BorderBrush = System.Windows.Media.Brushes.LightGray,
                        BorderThickness = new Thickness(1),
                        Child = chart
                    }
                };

                IslandChartsPanel.Children.Add(expander);
            }
        }
    }
}
