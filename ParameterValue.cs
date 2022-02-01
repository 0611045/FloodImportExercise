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

        /// <summary>
        /// Get the value as a list of decimals.
        /// </summary>
        /// <param name="countExpected"></param>
        /// <returns></returns>
        public List<decimal> GetAsDecimalList(int countExpected)
        {
            var value = this.Value.Split(',').Select(a => JbaPrecipitationFileReader.ConvertStringToType<decimal>(valueString: a)).ToList();

            if (value.Count != countExpected)
                throw new Exception($"Expected {countExpected} items but there are {value.Count}.");

            return value;
        }

        /// <summary>
        /// Get the value as a list of integers.
        /// </summary>
        /// <param name="separator"></param>
        /// <param name="countExpected"></param>
        /// <returns></returns>
        public List<int> GetAsIntegerList(char separator, int countExpected)
        {
            var value = this.Value.Split(separator).Select(a => JbaPrecipitationFileReader.ConvertStringToType<int>(valueString: a)).ToList();

            if (value.Count != countExpected)
                throw new Exception($"Expected {countExpected} items but there are {value.Count}.");

            return value;
        }

        /// <summary>
        /// Get the value as an extent.
        /// </summary>
        /// <returns></returns>
        public Extent GetAsExtent()
        {
            var decimalList = GetAsDecimalList(countExpected: 2);

            return new Extent(from: decimalList[0], to: decimalList[1]);
        }

        /// <summary>
        /// Get the value as an ExtentSize.
        /// </summary>
        /// <returns></returns>
        public ExtentSize GetAsExtentSize()
        {
            var integerList = GetAsIntegerList(separator: ',', countExpected: 2);

            return new ExtentSize(x: integerList[0], y: integerList[1]);
        }

        /// <summary>
        /// Get the value as a Year Range.
        /// </summary>
        /// <returns></returns>
        public TimePeriodYears GetAsYearRange()
        {
            var integerList = GetAsIntegerList(separator: '-', countExpected: 2);

            var item = new TimePeriodYears(from: integerList[0], to: integerList[1]);

            return item;
        }

        /// <summary>
        /// Get the value as a single value of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAsSingleValue<T>()
        {
            var valueInteger = JbaPrecipitationFileReader.ConvertStringToType<T>(valueString: this.Value);

            return valueInteger;
        }
    }
}
