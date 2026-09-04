using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions
{
    public class TestFunctionFactory
    {
        private static readonly Dictionary<TestFunctionType, TestFunction> FunctionMap = new()
        {
            { TestFunctionType.Rastrigin, new(TestFunctionType.Rastrigin, Rastrigin, -5.12, 5.12) },
            { TestFunctionType.Ackley, new(TestFunctionType.Ackley, Ackley, -32.768, 32.768) },
            { TestFunctionType.Rosenbrock, new(TestFunctionType.Rosenbrock, Rosenbrock, -5, 10) },
            { TestFunctionType.RosenbrockAltDomain, new(TestFunctionType.RosenbrockAltDomain, Rosenbrock, -2.048, 2.048) },
            { TestFunctionType.Griewank, new(TestFunctionType.Griewank, Griewank, -600, 600) },
            { TestFunctionType.Schwefel, new(TestFunctionType.Schwefel, Schwefel, -500, 500) },
            { TestFunctionType.Levy, new(TestFunctionType.Levy, Levy, -10, 10) },
            { TestFunctionType.Zakharov, new(TestFunctionType.Zakharov, Zakharov, -5, 10) },
            { TestFunctionType.SpinWaitL1Symmetric, new(TestFunctionType.SpinWaitL1Symmetric, SpinWaitL1, -10, 10) },
            { TestFunctionType.SpinWaitL2Symmetric, new(TestFunctionType.SpinWaitL2Symmetric, SpinWaitL2, -10, 10) },
            { TestFunctionType.SpinWaitL1Positive, new(TestFunctionType.SpinWaitL1Positive, SpinWaitL1, 0, 10) },
            { TestFunctionType.SpinWaitL2Positive, new(TestFunctionType.SpinWaitL2Positive, SpinWaitL2, 0, 10) },
            { TestFunctionType.SleepL1Symmetric, new(TestFunctionType.SleepL1Symmetric, SleepL1, -10, 10) },
            { TestFunctionType.SleepL2Symmetric, new(TestFunctionType.SleepL2Symmetric, SleepL2, -10, 10) },
            { TestFunctionType.SleepL1Positive, new(TestFunctionType.SleepL1Positive, SleepL1, 0, 10) },
            { TestFunctionType.SleepL2Positive, new(TestFunctionType.SleepL2Positive, SleepL2, 0, 10) },
        };
        
        public static TestFunction GetFunction(TestFunctionType type) => FunctionMap[type];
        
        public static double Ackley(double[] genes)
        {
            double a = 20, b = 0.2, c = 2 * Math.PI;
            int d = genes.Length;
            double sum1 = 0;
            double sum2 = 0;

            for (int i = 0; i < d; i++)
            {
                sum1 += genes[i] * genes[i];
                sum2 += Math.Cos(c * genes[i]);
            }

            return -a * Math.Exp(-b * Math.Sqrt(sum1 / d)) - Math.Exp(sum2 / d) + a + Math.E;
        }

        public static double Rastrigin(double[] genes)
        {
            double sum = 0;

            for (int i = 0; i < genes.Length; i++)
            {
                sum += genes[i] * genes[i] - 10 * Math.Cos(2 * Math.PI * genes[i]);
            }

            return 10 * genes.Length + sum;
        }

        public static double Rosenbrock(double[] genes)
        {
            double sum = 0;
            for (int i = 0; i < genes.Length - 1; i++)
            {
                sum += 100 * Math.Pow(genes[i + 1] - genes[i] * genes[i], 2) + Math.Pow(genes[i] - 1, 2);
            }

            return sum;
        }

        public static double Griewank(double[] genes)
        {
            double sum = genes.Sum(xi => xi * xi) / 4000.0;
            double prod = 1.0;

            for (int i = 0; i < genes.Length; i++)
            {
                sum += genes[i] * genes[i];
                prod *= Math.Cos(genes[i] / Math.Sqrt(i + 1));
            }

            return sum / 4000.0 - prod + 1;
        }

        public static double Schwefel(double[] genes)
        {
            double sum = 0;
            
            for (int i = 0; i < genes.Length; i++)
            {
                sum += genes[i] * Math.Sin(Math.Sqrt(Math.Abs(genes[i])));
            }

            return 418.9829 * genes.Length - sum;
        }

        public static double Levy(double[] genes)
        {
            int d = genes.Length;

            double w(int i) => 1 + (genes[i] - 1) / 4.0;
            double sum = Math.Sin(Math.PI * w(0)) * Math.Sin(Math.PI * w(0));

            for (int i = 0; i < d - 1; i++)
            {
                double wi = w(i);
                sum += (wi - 1) * (wi - 1) * (1 + 10 * Math.Sin(Math.PI * wi + 1) * Math.Sin(Math.PI * wi + 1));
            }

            double wd = w(d - 1);
            sum += (wd - 1) * (wd - 1) * (1 + Math.Sin(2 * Math.PI * wd) * Math.Sin(2 * Math.PI * wd));

            return sum;
        }

        public static double Zakharov(double[] genes)
        {
            double sum1 = 0.0;
            double sum2 = 0.0;

            for (int i = 0; i < genes.Length; i++)
            {
                sum1 += genes[i] * genes[i];
                sum2 += 0.5 * (i + 1) * genes[i];
            }

            return sum1 + Math.Pow(sum2, 2) + Math.Pow(sum2, 4);
        }

        public static double SpinWaitL1(double[] genes)
        {
            Thread.SpinWait(genes.Length * 25000);
            double sum = 0.0;
            for (int i = 0; i < genes.Length; i++)
            {
                sum += Math.Abs(genes[i]);
            }
            return sum;
        }

        public static double SpinWaitL2(double[] genes)
        {
            Thread.SpinWait(genes.Length * 25000);
            double sum = 0.0;
            for (int i = 0; i < genes.Length; i++)
            {
                sum += genes[i] * genes[i];
            }
            return sum;
        }

        public static double SleepL1(double[] genes)
        {
            Thread.Sleep(genes.Length / 2);
            double sum = 0.0;
            for (int i = 0; i < genes.Length; i++)
            {
                sum += Math.Abs(genes[i]);
            }
            return sum;
        }

        public static double SleepL2(double[] genes)
        {
            Thread.Sleep(genes.Length / 2);
            double sum = 0.0;
            for (int i = 0; i < genes.Length; i++)
            {
                sum += genes[i] * genes[i];
            }
            return sum;
        }
    }
}
