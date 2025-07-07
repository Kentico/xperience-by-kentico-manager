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


        public Task<int> ExecuteNonQuery(string connectionString, string queryText)
        {
            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand(queryText, connection);
            connection.Open();

            return command.ExecuteNonQueryAsync();
        }


        private static Task<string> GetSqlQueryText(string queryName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string? resourceName = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith(queryName));
            if (string.IsNullOrEmpty(resourceName))
            {
                throw new InvalidOperationException($"Resource '{queryName}' not found.");
            }

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);

            return reader.ReadToEndAsync();
        }
    }
}
