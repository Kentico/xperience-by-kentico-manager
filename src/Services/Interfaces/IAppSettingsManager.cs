using Xperience.Manager.Configuration;

namespace Xperience.Manager.Services
{
    /// <summary>
    /// Contains methods for interfacing with the application settings of a project.
    /// </summary>
    public interface IAppSettingsManager : IService
    {
        /// <summary>
        /// Gets the provided connection string.
        /// </summary>
        /// <param name="profile">The tool profile.</param>
        /// <param name="connectionStringName">The name of the connection string to retrieve.</param>
        /// <param name="fileName">The application settings file to read. If not provided, the appsettings.json file is used.</param>
        public Task<string?> GetConnectionString(ToolProfile? profile, string connectionStringName, string? fileName = null);


        /// <summary>
        /// Gets the headless options. See
        /// <see href="https://docs.xperience.io/xp/developers-and-admins/configuration/headless-channel-management#Headlesschannelmanagement-ConfiguretheheadlessAPI"/>.
        /// </summary>
        /// <param name="profile">The tool profile.</param>
        /// <param name="fileName">The application settings file to read. If not provided, the appsettings.json file is used.</param>
        public Task<CmsHeadlessConfiguration> GetCmsHeadlessConfiguration(ToolProfile? profile, string? fileName = null);


        /// <summary>
        /// Gets configurations keys with their <see cref="ConfigurationKey.ActualValue"/> set if available. See
        /// <see href="https://docs.xperience.io/xp/developers-and-admins/configuration/reference-configuration-keys"/>.
        /// </summary>
        /// <param name="profile">The tool profile.</param>
        /// <param name="keys">The keys to retrieve.</param>
        /// <param name="fileName">The application settings file to read. If not provided, the appsettings.json file is used.</param>
        public Task<IEnumerable<ConfigurationKey>> GetConfigurationKeys(
            ToolProfile? profile,
            IEnumerable<ConfigurationKey> keys,
            string? fileName = null);


        /// <summary>
        /// Writes the headless configuration to the application settings.
        /// </summary>
        /// <param name="profile">The tool profile.</param>
        /// <param name="headlessConfiguration">The configuration to write.</param>
        /// <param name="fileName">The application settings file to write to. If not provided, the appsettings.json file is used.</param>
        public Task SetCmsHeadlessConfiguration(
            ToolProfile? profile,
            CmsHeadlessConfiguration headlessConfiguration,
            string? fileName = null);


        /// <summary>
        /// Writes the connection string to the application settings.
        /// </summary>
        /// <param name="profile">The tool profile.</param>
        /// <param name="connectionStringName">The name of the connection string to write.</param>
        /// <param name="connectionString">The value of the connection string.</param>
        /// <param name="fileName">The application settings file to write. If not provided, the appsettings.json file is used.</param>
        public Task SetConnectionString(
            ToolProfile? profile,
            string connectionStringName,
            string connectionString,
            string? fileName = null);


        /// <summary>
        /// Writes a configuration key value to the application settings.
        /// </summary>
        /// <param name="profile">The tool profile.</param>
        /// <param name="keyName">The name of the key to write.</param>
        /// <param name="value">The value of the key.</param>
        /// <param name="fileName">The application settings file to write. If not provided, the appsettings.json file is used.</param>
        public Task SetKeyValue(ToolProfile? profile, string keyName, object value, string? fileName = null);
    }
}
