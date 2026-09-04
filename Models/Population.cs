namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models
{
    public class Population(Individual[] individuals)
    {
        public Individual[] Individuals { get; set; } = individuals;

        public Population Clone()
        {
            int n = Individuals.Length;
            Individual[] cloned = new Individual[n];

            for (int i = 0; i < n; i++)
            {
                cloned[i] = Individuals[i].Clone();
            }

            return new Population(cloned);
        }
    }
}
