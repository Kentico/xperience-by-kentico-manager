using System.Data;

using Microsoft.Data.SqlClient;

namespace Xperience.Manager.Services
{
    public class ConnectionFactory : IConnectionFactory
    {
        public IDbConnection GetConnection(string connectionString) => new SqlConnection(connectionString);
    }
}
