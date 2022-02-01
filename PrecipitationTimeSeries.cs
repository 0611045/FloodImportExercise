using System.Collections.Generic;
using JBAExercise.Console.Metadata;

namespace JBAExercise.Console
{
    public class PrecipitationTimeSeries
    {
        public ExtentSize Position { get; }
        public List<PrecipitationMonth> Values { get; }

        public PrecipitationTimeSeries(ExtentSize position, List<PrecipitationMonth> values)
        {
            this.Position = position;
            this.Values = values;
        }
    }
}