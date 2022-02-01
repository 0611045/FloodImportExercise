using System.Collections.Generic;
using System.Linq;

namespace JBAExercise.Console
{
    class Program
    {
        /// <summary>
        /// File path.
        /// </summary>
        private static string _filePath = $@"C:\Users\Tom Garner\OneDrive\Career\Applications\JBA\cru-ts-2-10.1991-2000-cutdown.pre";

        /// <summary>
        /// Main code entry point.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Create an instance of the JBA Precipitation File Reader and Writer (to MSSQL).
            using (var reader = new JbaPrecipitationFileReader(filePath: _filePath))
            using (var writer = new JbaPrecipitationFileMsSqlWriter(
                serverName: Settings.SqlServerName,
                databaseName: Settings.SqlDatabaseName,
                schemaName: Settings.SqlSchemaName,
                tableName: Settings.SqlTableName))
            {
                writer.Write(reader.SelectMany(a => Map(a)));
            }
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
