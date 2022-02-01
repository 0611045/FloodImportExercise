using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace JBAExercise.Console
{
    /// <summary>
    /// Writes the JBA Precipitation File to a SQL table.
    /// </summary>
    internal class JbaPrecipitationFileMsSqlWriter : IDisposable
    {
        public string ServerName { get; }
        public string DatabaseName { get; }
        public string SchemaName { get; }
        public string TableName { get; }

        public JbaPrecipitationFileMsSqlWriter(string serverName, string databaseName, string schemaName, string tableName)
        {
            this.ServerName = serverName;
            this.DatabaseName = databaseName;
            this.SchemaName = schemaName;
            this.TableName = tableName;
        }

        /// <summary>
        /// Write a collection of Data records.
        /// </summary>
        /// <param name="items"></param>
        internal void Write(IEnumerable<DataDb> items)
        {
            using (var sqlConnection = GetSqlConnection())
            {
                sqlConnection.Open();

                using (var sqlWriter = new SqlWriter<DataDb>(
                    connection: sqlConnection,
                    schemaName: this.SchemaName,
                    tableName: this.TableName))
                {
                    sqlWriter.Write(items);
                }
            }
        }

        /// <summary>
        /// Create a SQL connection string.
        /// </summary>
        /// <returns></returns>
        private string GetSqlConnectionString()
        {
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder()
            {
                DataSource = this.ServerName,
                InitialCatalog = this.DatabaseName,
                IntegratedSecurity = true
            };

            return builder.ConnectionString;
        }

        /// <summary>
        /// Get a SQL connection.
        /// </summary>
        /// <returns></returns>
        private SqlConnection GetSqlConnection()
        {
            var connStr = GetSqlConnectionString();
            return new SqlConnection(connStr);
        }

        public void Dispose()
        {

        }

        /// <summary>
        /// Utility class for writing data from a collection to SQL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal class SqlWriter<T> : IDisposable
        {
            private static readonly int _countBatchSize = 100000;

            private readonly List<PropertyInfo> _properties;
            private readonly string _schemaName;
            private readonly SqlConnection _sqlConnection;
            private readonly string _tableName;

            public SqlWriter(
                SqlConnection connection,
                string schemaName,
                string tableName)
            {
                _sqlConnection = connection;
                _schemaName = schemaName;
                _tableName = tableName;
                _properties = typeof(T).GetProperties().ToList();
            }

            public void Dispose()
            {
            }

            /// <summary>
            /// Write the items.
            /// </summary>
            /// <param name="items"></param>
            /// <returns></returns>
            public int Write(IEnumerable<T> items)
            {
                CreateTable();

                // Create a Data Table.
                var dataTable = GetDataTable();

                // Loop through all items.
                var countItems = 0;
                var countItemsWritten = 0;
                foreach (var item in items)
                {
                    var row = dataTable.NewRow();

                    foreach (var property in _properties)
                    {
                        var value = property.GetValue(item);

                        row[property.Name] = value ?? DBNull.Value;
                    }

                    dataTable.Rows.Add(row);

                    countItems++;

                    if (dataTable.Rows.Count > _countBatchSize)
                        countItemsWritten += Write(dataTable);
                }

                if (dataTable.Rows.Count > 0)
                    countItemsWritten += Write(dataTable);

                return countItemsWritten;
            }

            /// <summary>
            /// Create the table.
            /// </summary>
            private void CreateTable()
            {
                var sql = new List<string>() { $"IF EXISTS ( SELECT * FROM sys.tables WHERE name = '{_tableName}' ) DROP TABLE [{_schemaName}].[{_tableName}]; CREATE TABLE [{_schemaName}].[{_tableName}]" };
                sql.Add("(");

                var sqlFields = new List<string>();
                foreach (var item in _properties)
                {
                    // Get the Name of the Property.
                    var name = item.Name;

                    // Get the Data Type.
                    var dataType = MapToSql(item.PropertyType);

                    sqlFields.Add($"[{name}] {dataType}");
                }

                sql.Add(string.Join($",{Environment.NewLine}", sqlFields));
                sql.Add(")");

                using (var sqlCommand = new SqlCommand(string.Join("", sql)))
                {
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.Connection = _sqlConnection;

                    sqlCommand.ExecuteNonQuery();
                }
            }

            /// <summary>
            /// Map a .NET property to a SQL data type. For example, .NET DateTime == SQL DATE.
            /// </summary>
            /// <param name="property"></param>
            /// <returns></returns>
            private string MapToSql(System.Type property)
            {
                if (property == typeof(int))
                    return "INT";
                else if (property == typeof(DateTime))
                    return "DATE";
                else if (property == typeof(float))
                    return "REAL";
                else
                    throw new NotSupportedException();
            }

            /// <summary>
            /// Get a DataTable which describes the list of properties in the class described by T.
            /// </summary>
            /// <returns></returns>
            private DataTable GetDataTable()
            {
                var dataTable = new DataTable();

                dataTable.TableName = _tableName;

                // Loop through all Properties.
                foreach (var property in _properties)
                {
                    var propertyName = property.Name;

                    var underlyingType = System.Nullable.GetUnderlyingType(property.PropertyType);
                    var isNullable = underlyingType != null || property.PropertyType == typeof(string);
                    var type = underlyingType == null ? property.PropertyType : underlyingType;

                    var dataColumn = new DataColumn
                    {
                        ColumnName = property.Name,
                        DataType = type,
                        AllowDBNull = isNullable
                    };

                    dataTable.Columns.Add(dataColumn);
                }

                return dataTable;
            }

            /// <summary>
            /// Write the data to the Table using the SqlBulkCopy function.
            /// </summary>
            /// <param name="dataTable"></param>
            private int Write(DataTable dataTable)
            {
                var rowsWritten = 0;

                SqlBulkCopy sqlBulkCopy = null;

                // Write the data to the server.
                sqlBulkCopy = new SqlBulkCopy(_sqlConnection);

                // Set the Table ServerName.
                sqlBulkCopy.DestinationTableName = $"[{_schemaName}].[{_tableName}]";

                // Write the data.
                rowsWritten += dataTable.Rows.Count;

                sqlBulkCopy.WriteToServer(dataTable);

                // Clear the rows.
                dataTable.Rows.Clear();

                return rowsWritten;
            }
        }

        /// <summary>
        /// Class used as a mapper to the SQL table.
        /// </summary>
        internal class DataDb
        {
            public int Xref { get; }
            public int Yref { get; }

            [DataType(DataType.Date)]
            public DateTime Date { get; }

            public float Value { get; }

            public DataDb()
            {

            }

            public DataDb(int xref, int yref, DateTime date, float value)
            {
                this.Xref = xref;
                this.Yref = yref;
                this.Date = date;
                this.Value = value;
            }
        }
    }
}