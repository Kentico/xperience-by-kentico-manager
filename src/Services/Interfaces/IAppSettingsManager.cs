using Xperience.Manager.Configuration;

namespace Xperience.Manager.Services
{
    /// <summary>
    /// Contains methods for interfacing with the appsettings.json of a project.
    /// </summary>
    public interface IAppSettingsManager : IService
    {
        /// <summary>
        /// Gets the value of the connection string specified by the <paramref name="name"/>.
        /// </summary>
        public Task<string?> GetConnectionString(ToolProfile? profile, string name);


        /// <summary>
        /// Gets the headless options. See
        /// <see href="https://docs.xperience.io/xp/developers-and-admins/configuration/headless-channel-management#Headlesschannelmanagement-ConfiguretheheadlessAPI"/>.
        /// </summary>
        public Task<CmsHeadlessConfiguration> GetCmsHeadlessConfiguration(ToolProfile? profile);


        /// <summary>
        /// Gets configurations keys with their <see cref="ConfigurationKey.ActualValue"/> set if available. See
        /// <see href="https://docs.xperience.io/xp/developers-and-admins/configuration/reference-configuration-keys"/>.
        /// </summary>
        /// <param name="keys">The keys to retrieve.</param>
        public Task<IEnumerable<ConfigurationKey>> GetConfigurationKeys(ToolProfile? profile, IEnumerable<ConfigurationKey> keys);


        /// <summary>
        /// Writes the headless configuration to the appsettings.json.
        /// </summary>
        public Task SetCmsHeadlessConfiguration(ToolProfile? profile, CmsHeadlessConfiguration headlessConfiguration);


        /// <summary>
        /// Writes the connection string to the appsettings.json.
        /// </summary>
        public Task SetConnectionString(ToolProfile? profile, string name, string connectionString);


        /// <summary>
        /// Writes a configuration key value to the appsettings.json.
        /// </summary>
        public Task SetKeyValue(ToolProfile? profile, string keyName, object value);
    }
}
