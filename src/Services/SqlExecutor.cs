using Microsoft.Data.SqlClient;

using Newtonsoft.Json.Linq;

using Spectre.Console;

using System.Reflection;

namespace Xperience.Manager.Services
{
    public class SqlExecutor : ISqlExecutor
    {
        public async Task<IEnumerable<JObject>> ExecuteQuery(string connectionString, string queryName)
        {
            var result = new List<JObject>();
            string? query = await GetSqlQueryText(queryName);
            if (string.IsNullOrEmpty(query))
            {
                return result;
            }

            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand(query, connection);
            connection.Open();
            var reader = await command.ExecuteReaderAsync();
            var columns = await reader.GetColumnSchemaAsync();
            try
            {
                while (reader.Read())
                {
                    var row = new JObject();
                    foreach (string col in columns.Select(c => c.ColumnName))
                    {
                        object? value = reader[col];
                        if (value is null)
                        {
                            continue;
                        }

                        row.Add(col, JToken.FromObject(value));
                    }

                    result.Add(row);
                }
            }
            finally
            {
                reader.Close();
            }

            return result;
        }


        public async Task<int> ExecuteNonQuery(string connectionString, string queryText)
        {
            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand(queryText, connection);
            connection.Open();
            try
            {
                return await command.ExecuteNonQueryAsync();
            }
            finally
            {
                connection.Close();
            }
        }


        private static Task<string> GetSqlQueryText(string queryName)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string? executingDirectory = Path.GetDirectoryName(assemblyPath);
            string fullPathToScript = $"{executingDirectory}/Scripts/{queryName}.sql";

            return File.ReadAllTextAsync(fullPathToScript);
        }
    }
}
