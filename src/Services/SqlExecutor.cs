using Newtonsoft.Json.Linq;

using System.Reflection;

namespace Xperience.Manager.Services
{
    public class SqlExecutor(IConnectionFactory connectionFactory) : ISqlExecutor
    {
        public async Task<IEnumerable<JObject>> ExecuteQuery(string connectionString, string queryName)
        {
            var result = new List<JObject>();
            string? query = await GetSqlQueryText(queryName);
            if (string.IsNullOrEmpty(query))
            {
                return result;
            }

            using var connection = connectionFactory.GetConnection(connectionString);
            var command = connection.CreateCommand();
            command.CommandText = query;
            connection.Open();
            var reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var row = new JObject();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        object? value = reader.GetValue(i);
                        if (value is null)
                        {
                            continue;
                        }

                        row.Add(reader.GetName(i), JToken.FromObject(value));
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


        public async Task<int> ExecuteNonQuery(string connectionString, string queryName, IDictionary<string, object>? parameters = null)
        {
            string? query = await GetSqlQueryText(queryName);
            if (string.IsNullOrEmpty(query))
            {
                return 0;
            }

            using var connection = connectionFactory.GetConnection(connectionString);
            var command = connection.CreateCommand();
            command.CommandText = query;
            if (parameters is not null && parameters.Any())
            {
                foreach (var param in parameters)
                {
                    var newParam = command.CreateParameter();
                    newParam.ParameterName = param.Key;
                    newParam.Value = param.Value;
                    command.Parameters.Add(newParam);
                }
            }

            connection.Open();

            return command.ExecuteNonQuery();
        }


        private static Task<string> GetSqlQueryText(string queryName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string? resourceName = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith(queryName));
            if (string.IsNullOrEmpty(resourceName))
            {
                throw new InvalidOperationException($"Resource '{queryName}' not found.");
            }

            using var stream = assembly.GetManifestResourceStream(resourceName) ??
                throw new InvalidOperationException($"Failed to open stream to '{resourceName}'");
            using var reader = new StreamReader(stream);

            return reader.ReadToEndAsync();
        }
    }
}
