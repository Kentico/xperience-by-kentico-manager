using Xperience.Manager.Options;

namespace Xperience.Manager.Services
{
    /// <summary>
    /// Contains methods for managing Continuous Deployment XML configuration files.
    /// </summary>
    public interface ICDXmlManager : IService
    {
        /// <summary>
        /// Loads the provided Continuous Deployment XML file.
        /// </summary>
        public Task<RepositoryConfiguration?> GetConfig(string path);


        /// <summary>
        /// Saves the provided configuration to a physical XML file.
        /// </summary>
        public void WriteConfig(RepositoryConfiguration config, string path);
    }
}
