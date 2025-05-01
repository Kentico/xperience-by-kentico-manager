using Microsoft.Data.SqlClient;

using Newtonsoft.Json.Linq;

using Spectre.Console;

using System.Reflection;

namespace Xperience.Manager.Services
{
    public class SqlExecutor : ISqlExecutor
    {
        private const int MAX_COLUMN_CHARS = 200;


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


        public async Task<Table> GetTable(string connectionString, string queryName)
        {
            var table = new Table() { Border = TableBorder.Minimal };
            var result = await ExecuteQuery(connectionString, queryName);
            if (!result.Any())
            {
                return table;
            }

            var firstRow = result.FirstOrDefault();
            if (firstRow is null)
            {
                return table;
            }

            table.AddColumns(firstRow.Properties().Select(p => $"[{Constants.PROMPT_COLOR}]{p.Name}[/]").ToArray());
            foreach (var row in result)
            {
                var rowValues = row.Values().Select(GetFormattedValue);
                table
                    .AddEmptyRow() // Add an empty row to simulate padding
                    .AddRow(rowValues.ToArray());
            }

            return table;
        }


        private string GetFormattedValue(JToken token)
        {
            string stringValue = token.Value<string>() ?? string.Empty;
            stringValue = stringValue.Length > MAX_COLUMN_CHARS
                ? stringValue[..MAX_COLUMN_CHARS]
                : stringValue;

            return Markup.Escape(stringValue);
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
