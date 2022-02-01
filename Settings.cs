using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBAExercise.Console
{
    public static class Settings
    {
        public static string SqlServerName { get; internal set; }
        public static string SqlDatabaseName { get; internal set; }
        public static string SqlSchemaName { get; internal set; }
        public static string SqlTableName { get; internal set; }

        static Settings()
        {
            SqlServerName = GetAppSetting("SqlServerName");
            SqlDatabaseName = GetAppSetting("SqlDatabaseName");
            SqlSchemaName = GetAppSetting("SqlSchemaName");
            SqlTableName = GetAppSetting("SqlTableName");
        }

        private static string GetAppSetting(string name)
        {
            return System.Configuration.ConfigurationManager.AppSettings[name];
        }
    }
}
