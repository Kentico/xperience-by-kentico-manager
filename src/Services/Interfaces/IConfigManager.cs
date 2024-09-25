using Xperience.Manager.Configuration;
using Xperience.Manager.Options;

namespace Xperience.Manager.Services
{
    /// <summary>
    /// Contains methods for managing <see cref="ToolConfiguration"/>.
    /// </summary>
    public interface IConfigManager : IService
    {
        /// <summary>
        /// Ensures that the configuration file is present in the executing directory.
        /// </summary>
        public Task EnsureConfigFile();


        /// <summary>
        /// Adds a profile to the <see cref="ToolConfiguration.Profiles"/>.
        /// </summary>
        public Task AddProfile(ToolProfile? profile);


        /// <summary>
        /// Gets the current profile, or <c>null</c> if not set. If there is only one profile registered, that
        /// profile is automatically selected.
        /// </summary>
        public Task<ToolProfile?> GetCurrentProfile();


        /// <summary>
        /// Gets the <see cref="ToolConfiguration"/> represented by the physical file.
        /// </summary>
        public Task<ToolConfiguration> GetConfig();


        /// <summary>
        /// Gets the <see cref="InstallProjectOptions"/> specified by the tool configuration file, or a new instance if
        /// the configuration can't be read.
        /// </summary>
        public Task<InstallProjectOptions> GetDefaultInstallProjectOptions();


        /// <summary>
        /// Gets the <see cref="InstallDatabaseOptions"/> specified by the tool configuration file, or a new instance if
        /// the configuration can't be read.
        /// </summary>
        public Task<InstallDatabaseOptions> GetDefaultInstallDatabaseOptions();


        /// <summary>
        /// Removes a profile to the <see cref="ToolConfiguration.Profiles"/>.
        /// </summary>
        public Task RemoveProfile(ToolProfile? profile);


        /// <summary>
        /// Sets the currently active profile.
        /// </summary>
        public Task SetCurrentProfile(ToolProfile? profile);
    }
}
