using Xperience.Manager.Commands;

namespace Xperience.Manager.Configuration
{
    /// <summary>
    /// Settings used when running <see cref="ContinuousDeploymentCommand"/> scripts.
    /// </summary>
    public class ContinuousDeploymentConfig
    {
        /// <summary>
        /// The absolute path to the Continuous Deployment configuration file.
        /// </summary>
        public string? ConfigPath { get; set; }


        /// <summary>
        /// The absolute path to the Continuous Deployment repository.
        /// </summary>
        public string? RepositoryPath { get; set; }
    }
}
