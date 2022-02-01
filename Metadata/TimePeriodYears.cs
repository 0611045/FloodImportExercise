namespace JBAExercise.Console.Metadata
{
    public class TimePeriodYears
    {
        public int From { get; }
        public int To { get; }

        public TimePeriodYears(int from, int to)
        {
            this.From = from;
            this.To = to;
        }
    }
}
