using System.Data;

namespace Xperience.Manager.Services
{
    /// <summary>
    /// Provides methods for retrieving an <see cref="IDbConnection"/>.
    /// </summary>
    public interface IConnectionFactory : IService
    {
        /// <summary>
        /// Gets a new database connection using the provided <paramref name="connectionString"/>.
        /// </summary>
        public IDbConnection GetConnection(string connectionString);
    }
}
