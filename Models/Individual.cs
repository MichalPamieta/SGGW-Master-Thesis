using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.FitnessFunctions;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models
{
    public class Individual
    {
        public double[] Genotype { get; set; }
        public double Fitness { get; set; }

        public Individual(int length)
        {
            Genotype = new double[length];
        }

        public Individual(double[] genes, TestFunction testFunction)
        {
            Genotype = genes;
            Fitness = testFunction.Function(genes);
        }

        public Individual Clone()
        {
            Individual clone = new(Genotype.Length);
            Array.Copy(Genotype, clone.Genotype, Genotype.Length);
            clone.Fitness = Fitness;

            return clone;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Individual other = (Individual)obj;

            const double epsilon = 1e-9;

            if (Genotype.Length != other.Genotype.Length)
            {  
                return false;
            }

            ReadOnlySpan<double> a = Genotype;
            ReadOnlySpan<double> b = other.Genotype;

            for (int i = 0; i < a.Length; i++)
            {
                if (Math.Abs(a[i] - b[i]) > epsilon)
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + Fitness.GetHashCode();
            foreach (double gene in Genotype)
            {
                hash = hash * 23 + gene.GetHashCode();
            }

            return hash;
        }

        public static bool operator ==(Individual a, Individual b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Individual a, Individual b)
        {
            return !(a == b);
        }
    }
}
