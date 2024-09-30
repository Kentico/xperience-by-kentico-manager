using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xperience.Manager.Configuration;
using Xperience.Manager.Options;

namespace Xperience.Manager.Services
{
    public class ConfigManager : IConfigManager
    {
        public Task AddProfile(ToolProfile? profile)
        {
            if (profile is null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            return AddProfileInternal(profile);
        }


        public Task SetCurrentProfile(ToolProfile? profile)
        {
            if (profile is null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            return SetCurrentProfileInternal(profile);
        }


        public async Task<ToolProfile?> GetCurrentProfile()
        {
            var config = await GetConfig();
            var match = config.Profiles.FirstOrDefault(p => p.ProjectName?.Equals(config.CurrentProfile, StringComparison.OrdinalIgnoreCase) ?? false);
            if (config.Profiles.Count == 1 &&
                (string.IsNullOrEmpty(config.CurrentProfile) || match is null))
            {
                // There's only 1 profile and there was no value stored in config, or non-matching value
                // Select the only profile and save it in the config
                var profile = config.Profiles.FirstOrDefault();
                if (profile is not null)
                {
                    await SetCurrentProfile(profile);

                    return profile;
                }
            }

            return match;
        }


        public async Task<ToolConfiguration> GetConfig()
        {
            if (!File.Exists(Constants.CONFIG_FILENAME))
            {
                throw new FileNotFoundException($"The configuration file {Constants.CONFIG_FILENAME} was not found.");
            }

            string text = await File.ReadAllTextAsync(Constants.CONFIG_FILENAME);
            var config = JsonConvert.DeserializeObject<ToolConfiguration>(text) ?? throw new JsonReaderException($"The configuration file {Constants.CONFIG_FILENAME} cannot be deserialized.");

            return config;
        }


        public async Task EnsureConfigFile()
        {
            var toolVersion = Assembly.GetExecutingAssembly().GetName().Version ??
                throw new InvalidOperationException("The tool version couldn't be retrieved.");
            if (File.Exists(Constants.CONFIG_FILENAME))
            {
                await MigrateConfig(toolVersion);
                return;
            }

            await WriteConfig(new ToolConfiguration
            {
                Version = toolVersion,
                DefaultInstallProjectOptions = new(),
                DefaultInstallDatabaseOptions = new()
            });
        }


        public async Task<InstallProjectOptions> GetDefaultInstallProjectOptions()
        {
            var config = await GetConfig();

            return config.DefaultInstallProjectOptions ?? new();
        }


        public async Task<InstallDatabaseOptions> GetDefaultInstallDatabaseOptions()
        {
            var config = await GetConfig();

            return config.DefaultInstallDatabaseOptions ?? new();
        }


        public Task RemoveProfile(ToolProfile? profile)
        {
            if (profile is null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            return RemoveProfileInternal(profile);
        }


        private async Task AddProfileInternal(ToolProfile profile)
        {
            var config = await GetConfig();
            if (config.Profiles.Any(p => p.ProjectName?.Equals(profile.ProjectName, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                throw new InvalidOperationException($"There is already a profile named '{profile.ProjectName}.'");
            }

            config.Profiles.Add(profile);

            await WriteConfig(config);
        }


        private async Task MigrateConfig(Version toolVersion)
        {
            var config = await GetConfig();
            if (config.Version?.Equals(toolVersion) ?? false)
            {
                return;
            }

            // Perform any migrations from old config version to new version here
            string text = await File.ReadAllTextAsync(Constants.CONFIG_FILENAME);
            var json = JsonConvert.DeserializeObject<JObject>(text) ??
                throw new InvalidOperationException("Unable to read configuration file for migration.");
            if ((config.Version?.ToString().Equals("4.0.0.0") ?? false) && toolVersion.ToString().Equals("5.0.0.0"))
            {
                Migrate40To50(json, config);
            }

            config.Version = toolVersion;

            await WriteConfig(config);
        }


        private async Task RemoveProfileInternal(ToolProfile profile)
        {
            var config = await GetConfig();

            // For some reason Profiles.Remove() didn't work, make a new list
            var newProfiles = new List<ToolProfile>();
            newProfiles.AddRange(config.Profiles.Where(p => !p.ProjectName?.Equals(profile.ProjectName, StringComparison.OrdinalIgnoreCase) ?? true));

            config.Profiles = newProfiles;

            await WriteConfig(config);
        }


        private async Task SetCurrentProfileInternal(ToolProfile profile)
        {
            var config = await GetConfig();
            config.CurrentProfile = profile.ProjectName;
            await WriteConfig(config);
        }


        private static Task WriteConfig(ToolConfiguration config) =>
            File.WriteAllTextAsync(Constants.CONFIG_FILENAME, JsonConvert.SerializeObject(config, Formatting.Indented));


        private static void Migrate40To50(JObject oldConfig, ToolConfiguration newConfig)
        {
            var oldInstallOptions = oldConfig["DefaultInstallOptions"];

            var dbOptions = new InstallDatabaseOptions();
            dbOptions.DatabaseName = oldInstallOptions?["DatabaseName"]?.ToString() ?? dbOptions.DatabaseName;
            dbOptions.ServerName = oldInstallOptions?["ServerName"]?.ToString() ?? dbOptions.ServerName;
            newConfig.DefaultInstallDatabaseOptions = dbOptions;

            var projectOptions = new InstallProjectOptions();
            projectOptions.Template = oldInstallOptions?["Template"]?.ToString() ?? projectOptions.Template;
            projectOptions.ProjectName = oldInstallOptions?["ProjectName"]?.ToString() ?? projectOptions.ProjectName;
            projectOptions.InstallRootPath = oldInstallOptions?["InstallRootPath"]?.ToString() ?? projectOptions.InstallRootPath;
            projectOptions.UseCloud = bool.Parse(oldInstallOptions?["UseCloud"]?.ToString() ?? projectOptions.UseCloud.ToString());
            string? oldVersion = oldInstallOptions?["Version"]?.ToString();
            if (!string.IsNullOrEmpty(oldVersion))
            {
                projectOptions.Version = Version.Parse(oldVersion);
            }

            newConfig.DefaultInstallProjectOptions = projectOptions;
        }
    }
}
