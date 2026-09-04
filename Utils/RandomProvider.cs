using System.Security.Cryptography;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils
{
    public static class RandomProvider
    {
        private static ThreadLocal<Random> threadLocal = new(() => new Random(GetBetterSeed()));

        public static void Initialize(int? seed = null)
        {
            if (seed.HasValue)
            {
                threadLocal = new ThreadLocal<Random>(() => new Random(seed.Value));
            }
            else
            {
                threadLocal = new ThreadLocal<Random>(() => new Random(GetBetterSeed()));
            }
        }

        public static Random Get()
        {
            return threadLocal.Value!;
        }

        private static int GetBetterSeed()
        {
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);

            return BitConverter.ToInt32(bytes, 0);
        }

        public static int Next() => threadLocal.Value!.Next();

        public static int Next(int maxValue) => threadLocal.Value!.Next(maxValue);

        public static int Next(int minValue, int maxValue) => threadLocal.Value!.Next(minValue, maxValue);

        public static double NextDouble() => threadLocal.Value!.NextDouble();
    }
}
