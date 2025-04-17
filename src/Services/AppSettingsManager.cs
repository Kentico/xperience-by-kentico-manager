using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xperience.Manager.Configuration;

namespace Xperience.Manager.Services
{
    public class AppSettingsManager : IAppSettingsManager
    {
        private readonly string cmsHeadlessSection = "CMSHeadless";


        public async Task<string?> GetConnectionString(ToolProfile? profile, string name)
        {
            var appSettings = await LoadSettings(profile);
            var connectionStrings = appSettings["ConnectionStrings"];
            if (connectionStrings is null)
            {
                return null;
            }

            return connectionStrings.Value<string>(name);
        }


        public async Task<CmsHeadlessConfiguration> GetCmsHeadlessConfiguration(ToolProfile? profile)
        {
            var appSettings = await LoadSettings(profile);
            var headlessConfig = appSettings.GetValue(cmsHeadlessSection)?.ToObject<CmsHeadlessConfiguration>();
            if (headlessConfig is null)
            {
                return new CmsHeadlessConfiguration();
            }

            return headlessConfig;
        }


        public async Task<IEnumerable<ConfigurationKey>> GetConfigurationKeys(ToolProfile? profile)
        {
            var appSettings = await LoadSettings(profile);
            var populatedKeys = Constants.ConfigurationKeys
                .Where(key => appSettings.Properties().Select(p => p.Name).Contains(key.KeyName, StringComparer.OrdinalIgnoreCase))
                .Select(key => { key.ActualValue = appSettings.GetValue(key.KeyName)?.Value<object>(); return key; });
            var allKeys = new List<ConfigurationKey>(populatedKeys);

            allKeys.AddRange(Constants.ConfigurationKeys.Where(key => !populatedKeys.Select(k => k.KeyName).Contains(key.KeyName)));

            return allKeys;
        }


        public async Task SetCmsHeadlessConfiguration(ToolProfile? profile, CmsHeadlessConfiguration headlessConfiguration)
        {
            var appSettings = await LoadSettings(profile);
            appSettings[cmsHeadlessSection] = JToken.FromObject(headlessConfiguration);

            await WriteAppSettings(profile, appSettings);
        }


        public async Task SetConnectionString(ToolProfile? profile, string name, string connectionString)
        {
            var appSettings = await LoadSettings(profile);
            var connectionStrings = appSettings["ConnectionStrings"]
                ?? throw new InvalidOperationException("ConnectionStrings section not found.");
            connectionStrings[name] = connectionString;

            await WriteAppSettings(profile, appSettings);
        }


        public async Task SetKeyValue(ToolProfile? profile, string keyName, object value)
        {
            var appSettings = await LoadSettings(profile);
            appSettings[keyName] = JToken.FromObject(value);

            await WriteAppSettings(profile, appSettings);
        }


        private static string GetAppSettingsPath(ToolProfile profile) => $"{profile.WorkingDirectory}/appsettings.json";


        private static Task<JObject> LoadSettings(ToolProfile? profile) =>
            profile is null ? throw new ArgumentNullException(nameof(profile)) : LoadSettingsInternal(profile);


        private static async Task<JObject> LoadSettingsInternal(ToolProfile profile)
        {
            string settingsPath = GetAppSettingsPath(profile);
            if (!File.Exists(settingsPath))
            {
                throw new InvalidOperationException($"Settings not found at {settingsPath}.");
            }

            string text = await File.ReadAllTextAsync(settingsPath);

            return JsonConvert.DeserializeObject<JObject>(text)
                ?? throw new InvalidOperationException("Failed to deserialize appsettings.json");
        }


        private static Task WriteAppSettings(ToolProfile? profile, JObject appSettings) =>
            profile is null ? throw new ArgumentNullException(nameof(profile)) : WriteAppSettingsInternal(profile, appSettings);


        private static async Task WriteAppSettingsInternal(ToolProfile profile, JObject appSettings)
        {
            string settingsPath = GetAppSettingsPath(profile);

            await File.WriteAllTextAsync(settingsPath, JsonConvert.SerializeObject(appSettings, Formatting.Indented));
        }
    }
}
