using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions
{
    public class TestFunction(TestFunctionType type, Func<double[], double> function, double min, double max)
    {
        public TestFunctionType Type { get; set; } = type;
        public Func<double[], double> Function { get; set; } = function;
        public double MinDomain { get; set; } = min;
        public double MaxDomain { get; set; } = max;
    }
}
