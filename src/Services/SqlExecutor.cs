using Microsoft.Data.SqlClient;

using Newtonsoft.Json.Linq;

using Spectre.Console;

using System.Reflection;

namespace Xperience.Manager.Services
{
    public class SqlExecutor : ISqlExecutor
    {
        public async Task<IEnumerable<JObject>> ExecuteQuery(string connectionString, string queryText)
        {
            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand(queryText, connection);
            connection.Open();
            var result = new List<JObject>();
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


        public async Task<Table> GetTable(string connectionString, string queryName)
        {
            var table = new Table();
            string? query = await GetSqlQueryText(queryName);
            if (string.IsNullOrEmpty(query))
            {
                return table;
            }

            var result = await ExecuteQuery(connectionString, query);
            if (!result.Any())
            {
                return table;
            }

            var firstRow = result.FirstOrDefault();
            if (firstRow is null)
            {
                return table;
            }

            table.AddColumns(firstRow.Properties().Select(p => p.Name).ToArray());
            foreach (var row in result)
            {
                var rowValues = row.Values().Select(v => v.Value<string>() ?? string.Empty);
                table.AddRow(rowValues.ToArray());
            }

            return table;
        }


        private static async Task<string> GetSqlQueryText(string queryName)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string? executingDirectory = Path.GetDirectoryName(assemblyPath);
            string fullPathToScript = $"{executingDirectory}/Scripts/{queryName}.sql";

            return await File.ReadAllTextAsync(fullPathToScript);
        }
    }
}
