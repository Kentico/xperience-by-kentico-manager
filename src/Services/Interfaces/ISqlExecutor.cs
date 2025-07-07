using Newtonsoft.Json.Linq;

namespace Xperience.Manager.Services
{
    /// <summary>
    /// Contains methods for executing SQL queries directly against a database.
    /// </summary>
    public interface ISqlExecutor : IService
    {
        /// <summary>
        /// Executes a SQL query and returns the rows as objects containing properties equal to the returned column names.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="queryName">The name of the SQL file in the ~/Scripts folder, with the extension.</param>
        Task<IEnumerable<JObject>> ExecuteQuery(string connectionString, string queryName);


        /// <summary>
        /// Executes a non-query and returns the number of affected rows.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="queryText">The SQL query text.</param>
        Task<int> ExecuteNonQuery(string connectionString, string queryText);
    }
}
