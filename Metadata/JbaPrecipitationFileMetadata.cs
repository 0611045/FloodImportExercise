namespace JBAExercise.Console.Metadata
{
    public class JbaPrecipitationFileMetadata
    {
        public string Title { get; }
        public string Units { get; }
        public string Version { get; }
        public Surface Grid { get; }
        public Other Other2 { get; }

        public JbaPrecipitationFileMetadata(string title, string units, string version, Surface grid, Other other)
        {
            this.Title = title;
            this.Units = units;
            this.Version = version;
            this.Grid = grid;
            this.Other2 = other;
        }
    }
}
