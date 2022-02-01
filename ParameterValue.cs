using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JBAExercise.Console.Metadata;

namespace JBAExercise.Console
{
    public class ParameterValue
    {
        public string Name { get; }
        public string Value { get; }

        public ParameterValue(string value)
        {
            this.Value = value;
        }

        public List<decimal> GetAsDecimalList(int countExpected)
        {
            var value = this.Value.Split(',').Select(a => JbaPrecipitationFileReader.ConvertStringToType<decimal>(valueString: a)).ToList();

            if (value.Count != countExpected)
                throw new Exception($"Expected {countExpected} items but there are {value.Count}.");

            return value;
        }

        public List<int> GetAsIntegerList(char separator, int countExpected)
        {
            var value = this.Value.Split(separator).Select(a => JbaPrecipitationFileReader.ConvertStringToType<int>(valueString: a)).ToList();

            if (value.Count != countExpected)
                throw new Exception($"Expected {countExpected} items but there are {value.Count}.");

            return value;
        }

        public Extent GetAsExtent()
        {
            var decimalList = GetAsDecimalList(countExpected: 2);

            return new Extent(from: decimalList[0], to: decimalList[1]);
        }

        public ExtentSize GetAsExtentSize()
        {
            var integerList = GetAsIntegerList(separator: ',', countExpected: 2);

            return new ExtentSize(x: integerList[0], y: integerList[1]);
        }

        public TimePeriodYears GetAsYearRange()
        {
            var integerList = GetAsIntegerList(separator: '-', countExpected: 2);

            var item = new TimePeriodYears(from: integerList[0], to: integerList[1]);

            return item;
        }

        public T GetAsSingleValue<T>()
        {
            var valueInteger = JbaPrecipitationFileReader.ConvertStringToType<T>(valueString: this.Value);

            return valueInteger;
        }
    }
}
