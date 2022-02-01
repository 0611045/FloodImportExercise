namespace JBAExercise.Console.Metadata
{
    public class Surface
    {
        public Extent Latitude { get; }
        public Extent Longitude { get; }
        public ExtentSize Size { get; }

        public Surface(Extent latitude, Extent longitude, ExtentSize size)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Size = size;
        }
    }
}
