using System;
using System.Collections.Generic;
using System.Linq;

namespace JBAExercise.Console
{
    class Program
    {
        /// <summary>
        /// Main code entry point.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                // Create an instance of the JBA Precipitation File Reader and Writer (to MSSQL).
                using (var reader = new JbaPrecipitationFileReader(filePath: Settings.FilePath))
                using (var writer = new JbaPrecipitationFileMsSqlWriter(
                    serverName: Settings.SqlServerName,
                    databaseName: Settings.SqlDatabaseName,
                    schemaName: Settings.SqlSchemaName,
                    tableName: Settings.SqlTableName))
                {
                    writer.Write(reader.SelectMany(a => Map(a)));
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }

            System.Console.WriteLine($"Finished writing the file.");
            System.Console.WriteLine($"Press return to exit.");
            System.Console.ReadLine();
        }

        // Map the Precipitation Time Series from a DTO to a DB object.
        private static IEnumerable<JbaPrecipitationFileMsSqlWriter.DataDb> Map(PrecipitationTimeSeries precipTimeSeries)
        {
            foreach(var item in precipTimeSeries.Values)
            {
                yield return new JbaPrecipitationFileMsSqlWriter.DataDb(
                    xref: precipTimeSeries.Position.X,
                    yref: precipTimeSeries.Position.Y,
                    date: new System.DateTime(year: item.Year, month: item.Month, day: 1),
                    value: item.Value);
            }
        }
    }
}
