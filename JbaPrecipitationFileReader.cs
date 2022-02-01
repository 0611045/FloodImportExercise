using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JBAExercise.Console.Metadata;

namespace JBAExercise.Console
{
    /// <summary>
    /// Read the JBA file.
    /// </summary>
    internal class JbaPrecipitationFileReader : IEnumerable<PrecipitationTimeSeries>, IDisposable
    {
        private StreamReader _streamReader;
        private ErrorLog _log;

        internal JbaPrecipitationFileMetadata Metadata { get; private set; }
        internal int LineNumber { get; private set; }
        internal string CurrentLine { get; private set; }

        internal JbaPrecipitationFileReader(string filePath)
        {
            _streamReader = new StreamReader(filePath);
            _log = new ErrorLog(this);
            this.Metadata = GetMetadata(streamReader: _streamReader);
        }

        public IEnumerator<PrecipitationTimeSeries> GetEnumerator()
        {
            while (!_streamReader.EndOfStream)
            {
                // Get and parse the "Grid-ref" line.
                var gridRefLine = ReadLines(count: 1).Single();
                var gridRef = ParseGridRef(line: gridRefLine);

                // Read and parse the Precipitation Time Series.
                var precipitationTimeSeries = ReadAndParseBox(timePeriod: this.Metadata.Other2.TimePeriod).ToList();

                // Return the Grid X and Y and the Precipitation Time Series in a single object.
                var box = new PrecipitationTimeSeries(position: gridRef, values: precipitationTimeSeries);

                yield return box;
            }
        }
         
        /// <summary>
        /// Reads the file's metadata.
        /// </summary>
        /// <param name="streamReader"></param>
        /// <returns></returns>
        private JbaPrecipitationFileMetadata GetMetadata(StreamReader streamReader)
        {
            // Read the firs three header lines, which appear to be free-text.
            var headerInfo = ReadLines(count: 3).ToList();
            var title = headerInfo[0];
            var units = headerInfo[1];
            var version = headerInfo[2];

            // Get the fouth and fith header lines, which are structured values in square brackets with parameter names.
            var headersStructuredParameters = ReadLines(count: 2).Select(a => GetParametersRawFromLine(line: a)).ToList();
            var grid = GetGrid(headersStructuredParameters[0]);
            var other = GetOther(headersStructuredParameters[1]);

            var metadata = new JbaPrecipitationFileMetadata(
                title: title,
                units: units,
                version: version,
                grid: grid,
                other: other);

            return metadata;
        }

        /// <summary>
        /// Reads the metadata's extent information (line 4).
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private Surface GetGrid(ParametersRaw parameters)
        {
            // Get the Long, Lati and Grid parts.
            var longitude = parameters.Get(name: "Long").GetAsExtent();
            var latitude = parameters.Get(name: "Lati").GetAsExtent();
            var size = parameters.Get(name: "Grid X,Y").GetAsExtentSize();

            var surface = new Surface(latitude: latitude, longitude: longitude, size: size);

            return surface;
        }

        /// <summary>
        /// Parses the metadata's Other information (line 5).
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private Other GetOther(ParametersRaw parameters)
        {
            var boxes = parameters.Get("Boxes").GetAsSingleValue<int>();
            var yearRange = parameters.Get("Years").GetAsYearRange();
            var multi = parameters.Get("Multi").GetAsSingleValue<decimal>();
            var missing = parameters.Get("Missing").GetAsSingleValue<decimal>();

            return new Other(
                countBoxes: boxes, 
                timePeriod: yearRange,
                multi: multi, 
                missing: missing);
        }

        /// <summary>
        /// Takes a header parameter, defined as information in the format [{name}={value}], and splits it up into name and value.
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private static GridParameterRaw ParseGridParameterRaw(string part)
        {
            // Split the string using the equals sign.
            var parts = part.Split('=').ToList();

            if (parts.Count != 2)
                throw new Exception($"Have {parts.Count}, but expected 2.");

            return new GridParameterRaw(name: parts[0], value: parts[1]);
        }

        /// <summary>
        /// Converts a string to the type specified in T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueString"></param>
        /// <returns></returns>
        internal static T ConvertStringToType<T>(string valueString)
        {
            try
            {
                if (typeof(T) == typeof(int))
                    return (T)Convert.ChangeType(ConvertStringToInt(valueString: valueString), typeof(T));
                else if (typeof(T) == typeof(decimal))
                    return (T)Convert.ChangeType(ConvertStringToDecimal(valueString: valueString), typeof(T));
                else
                    throw new NotSupportedException($"Type {typeof(T).Name} is not supported.");
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"The value {ex.Message} cannot be converted to type {typeof(T).Name}.");
            }
        }

        /// <summary>
        /// Convert a string to an integer.
        /// </summary>
        /// <param name="valueString"></param>
        /// <returns></returns>
        private static int ConvertStringToInt(string valueString)
        {
            int value = 0;
            if (!int.TryParse(valueString, out value))
                throw new Exception(valueString);
            return value;
        }

        /// <summary>
        /// Convert a string to a decimal.
        /// </summary>
        /// <param name="valueString"></param>
        /// <returns></returns>
        private static decimal ConvertStringToDecimal(string valueString)
        {
            decimal value = 0;
            if (!decimal.TryParse(valueString, out value))
                throw new Exception(valueString);
            return value;
        }
        
        /// <summary>
        /// Convert a header line with structured data (i.e. [{name1}={value1}] [{name2}={value2}] etc.) into a readable format.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static ParametersRaw GetParametersRawFromLine(string line)
        {
            // Split the parameters using the square brackets.
            var parametersRaw = SplitStartAndEnd(line: line, startCharacter: '[', endCharacter: ']').ToList();

            var things = new ParametersRaw(
                raws: parametersRaw.Select(a => ParseGridParameterRaw(a)).ToDictionary(a => a.Name, b => b.Value));

            return things;
        }

        /// <summary>
        /// Parse a Grid-ref record (i.e. 'Grid-ref=   1, 311').
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        internal static ExtentSize ParseGridRef(string line)
        {
            var raw = ParseGridParameterRaw(line);

            return raw.Value.GetAsExtentSize();
        }

        /// <summary>
        /// Reads an entire time-series for a single grid. I.e. the below:
        /// Grid-ref=   1, 148
        ///  3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630
        ///  3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630
        ///  3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630
        ///  3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630
        ///  3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630
        ///  3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630
        ///  3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630
        ///  3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630
        ///  3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630
        ///  3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630
        /// </summary>
        /// <param name="timePeriod"></param>
        /// <returns></returns>
        private IEnumerable<PrecipitationMonth> ReadAndParseBox(TimePeriodYears timePeriod)
        {
            for (int year = timePeriod.From; year <= timePeriod.To; year++)
            {
                // Read the values line.
                var line = ReadLines(count: 1).Single();

                // Split them into their months.
                var valuesByMonth = line.Split(' ').Where(a => !string.IsNullOrWhiteSpace(a)).ToList();

                // Check that there are 12 values. If there are not, report that there isn't 12 and skip the entire grid.
                if (valuesByMonth.Count != 12)
                     _log.Report($"There are {valuesByMonth.Count} columns, but 12 were expected.");
                else
                {
                    var valuesByMonthDecimal = new List<float>();
                    for (byte month = 1; month <= valuesByMonth.Count; month++)
                    {
                        var valueString = valuesByMonth[month - 1];

                        // Attempt to parse the value.
                        float valueFloat;
                        if (!float.TryParse(valueString, out valueFloat))
                            _log.Report($"Unable to convert value '{valueString}' to type decimal. Line: {this.LineNumber}, Month: {valueString}");
                        else
                            yield return new PrecipitationMonth(year: year, month: month, value: valueFloat);
                    }
                }
            }
        }

        /// <summary>
        /// Read a specified number of lines of the file.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private IEnumerable<string> ReadLines(int count)
        {
            for (int i = 1; i <= count; i++)
            {
                var line = _streamReader.ReadLine();
                this.CurrentLine = line;
                this.LineNumber++;
                yield return line;
            }
        }

        /// <summary>
        /// Split a string into it's constituent parts. Anything between starting and closing square brackets is considered one part.
        /// For example, header line 4 has three parts, header line 5 has four parts.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="startCharacter"></param>
        /// <param name="endCharacter"></param>
        /// <returns></returns>
        private static IEnumerable<string> SplitStartAndEnd(string line, char startCharacter, char endCharacter)
        {
            bool hasStarted = false;
            bool hasEnded = false;
            bool addCharacter = false;
            var characters = new List<char>();

            // Loop through all characters...
            foreach (var character in line.ToCharArray())
            {
                if (character == startCharacter)
                {
                    if (hasStarted)
                        throw new Exception("Start character encountered twice without end character.");
                    else
                        hasStarted = true;
                }
                else if (character == endCharacter)
                {
                    if (hasEnded)
                        throw new Exception("End character encountered twice without start character.");
                    else
                        hasEnded = true;
                }
                else
                    addCharacter = true;

                if (addCharacter)
                {
                    characters.Add(character);
                    addCharacter = false;
                }
                else if (hasStarted && hasEnded)
                {
                    yield return new string(characters.ToArray()).Trim();
                    characters = new List<char>();
                    hasStarted = false;
                    hasEnded = false;
                }
            }
        }

        public void Dispose()
        {
            _streamReader.Dispose();
            _log.Dispose();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private class GridParameterRaw
        {
            public string Name { get; }
            public ParameterValue Value { get; }

            public GridParameterRaw(string name, string value)
            {
                this.Name = name;
                this.Value = new ParameterValue(value: value);
            }
        }

        private class ParametersRaw
        {
            public Dictionary<string, ParameterValue> Raws { get; }

            public ParametersRaw(Dictionary<string, string> raws)
            {
                this.Raws = raws.ToDictionary(a => a.Key, b => new ParameterValue(value: b.Value));
            }

            public ParametersRaw(Dictionary<string, ParameterValue> raws)
            {
                this.Raws = raws.ToDictionary(a => a.Key, b => b.Value);
            }

            public ParameterValue Get(string name)
            {
                ParameterValue value = null;
                if (!this.Raws.TryGetValue(key: name, value: out value))
                    throw new Exception($"Cannot find parameter '{name}'.");

                return value;
            }
        }

        private class SingleValueParameter<T>
        {
            public string Name { get; }
            public T Value { get; }

            public SingleValueParameter(string name, T value)
            {
                this.Name = name;
                this.Value = value;
            }
        }
    }
}