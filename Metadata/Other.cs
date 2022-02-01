namespace JBAExercise.Console.Metadata
{
    public class Other
    {
        public int CountBoxes { get; }
        public TimePeriodYears TimePeriod { get; }
        public decimal Multi { get; }
        public decimal Missing { get; }

        public Other(int countBoxes, TimePeriodYears timePeriod, decimal multi, decimal missing)
        {
            this.CountBoxes = countBoxes;
            this.TimePeriod = timePeriod;
            this.Multi = multi;
            this.Missing = missing;
        }
    }
}
