using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xperience.Manager.Configuration;

namespace Xperience.Manager.Services
{
    public class AppSettingsManager : IAppSettingsManager
    {
        private const string CMS_HEADLESS_SECTION = "CMSHeadless";
        private const string DEFAULT_FILENAME = "appsettings.json";


        public async Task<string?> GetConnectionString(ToolProfile? profile, string connectionStringName, string? fileName = null)
        {
            var appSettings = await LoadSettings(profile, fileName);
            var connectionStrings = appSettings["ConnectionStrings"];
            if (connectionStrings is null)
            {
                return null;
            }

            return connectionStrings.Value<string>(connectionStringName);
        }


        public async Task<CmsHeadlessConfiguration> GetCmsHeadlessConfiguration(ToolProfile? profile, string? fileName = null)
        {
            var appSettings = await LoadSettings(profile, fileName);
            var headlessConfig = appSettings.GetValue(CMS_HEADLESS_SECTION)?.ToObject<CmsHeadlessConfiguration>();
            if (headlessConfig is null)
            {
                return new CmsHeadlessConfiguration();
            }

            return headlessConfig;
        }


        public async Task<IEnumerable<ConfigurationKey>> GetConfigurationKeys(
            ToolProfile? profile,
            IEnumerable<ConfigurationKey> keys,
            string? fileName = null)
        {
            var appSettings = await LoadSettings(profile, fileName);
            var populatedKeys = keys
                .Where(key => appSettings.Properties().Select(p => p.Name).Contains(key.KeyName, StringComparer.OrdinalIgnoreCase))
                .Select(key => { key.ActualValue = appSettings.GetValue(key.KeyName)?.Value<object>(); return key; });
            var allKeys = new List<ConfigurationKey>(populatedKeys);

            allKeys.AddRange(keys.Where(key => !populatedKeys.Select(k => k.KeyName).Contains(key.KeyName)));

            return allKeys;
        }


        public async Task SetCmsHeadlessConfiguration(
            ToolProfile? profile,
            CmsHeadlessConfiguration headlessConfiguration,
            string? fileName = null)
        {
            var appSettings = await LoadSettings(profile, fileName);
            appSettings[CMS_HEADLESS_SECTION] = JToken.FromObject(headlessConfiguration);

            await WriteAppSettings(profile, appSettings, fileName);
        }


        public async Task SetConnectionString(
            ToolProfile? profile,
            string connectionStringName,
            string connectionString,
            string? fileName = null)
        {
            var appSettings = await LoadSettings(profile, fileName);
            var connectionStrings = appSettings["ConnectionStrings"]
                ?? throw new InvalidOperationException("ConnectionStrings section not found.");
            connectionStrings[connectionStringName] = connectionString;

            await WriteAppSettings(profile, appSettings, fileName);
        }


        public async Task SetKeyValue(ToolProfile? profile, string keyName, object value, string? fileName = null)
        {
            var appSettings = await LoadSettings(profile, fileName);
            appSettings[keyName] = JToken.FromObject(value);

            await WriteAppSettings(profile, appSettings, fileName);
        }


        private static string GetAppSettingsPath(ToolProfile profile, string? fileName) =>
            $"{profile.WorkingDirectory}/{fileName ?? DEFAULT_FILENAME}";


        private static Task<JObject> LoadSettings(ToolProfile? profile, string? fileName) =>
            profile is null ? throw new ArgumentNullException(nameof(profile)) : LoadSettingsInternal(profile, fileName);


        private static async Task<JObject> LoadSettingsInternal(ToolProfile profile, string? fileName)
        {
            string settingsPath = GetAppSettingsPath(profile, fileName);
            if (!File.Exists(settingsPath))
            {
                throw new InvalidOperationException($"Settings not found at {settingsPath}.");
            }

            string text = await File.ReadAllTextAsync(settingsPath);

            return JsonConvert.DeserializeObject<JObject>(text)
                ?? throw new InvalidOperationException($"Failed to deserialize {fileName ?? DEFAULT_FILENAME}");
        }


        private static Task WriteAppSettings(ToolProfile? profile, JObject appSettings, string? fileName) =>
            profile is null ? throw new ArgumentNullException(nameof(profile)) : WriteAppSettingsInternal(profile, appSettings, fileName);


        private static async Task WriteAppSettingsInternal(ToolProfile profile, JObject appSettings, string? fileName)
        {
            string settingsPath = GetAppSettingsPath(profile, fileName);

            await File.WriteAllTextAsync(settingsPath, JsonConvert.SerializeObject(appSettings, Formatting.Indented));
        }
    }
}
