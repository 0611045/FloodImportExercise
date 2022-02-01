using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBAExercise.Console
{
    public class PrecipitationMonth
    {
        public int Year { get; }
        public byte Month { get; }
        public float Value { get; }

        public PrecipitationMonth(int year, byte month, float value)
        {
            this.Year = year;
            this.Month = month;
            this.Value = value;
        }
    }
}
