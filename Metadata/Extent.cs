namespace JBAExercise.Console.Metadata
{
    public class Extent
    {
        public decimal From { get; }
        public decimal To { get; }

        public Extent(decimal from, decimal to)
        {
            this.From = from;
            this.To = to;
        }
    }
}
