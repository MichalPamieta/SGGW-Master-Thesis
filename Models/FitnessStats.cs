namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models
{
    public readonly struct FitnessStats(double min, double max, double avg)
    {
        public readonly double Min = min;
        public readonly double Max = max;
        public readonly double Avg = avg;
    }
}
