using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

using Spectre.Console;

using Xperience.Manager.Configuration;
using Xperience.Manager.Options;
using Xperience.Manager.Services;
using Xperience.Manager.Wizards;

namespace Xperience.Manager.Commands
{
    /// <summary>
    /// A command which configures the appsettings.json of a project.
    /// </summary>
    public class SettingsCommand : AbstractCommand
    {
        private readonly IWizard<SettingsOptions> wizard;
        private readonly IAppSettingsManager appSettingsManager;


        public override IEnumerable<string> Keywords => ["s", "settings"];


        public override IEnumerable<string> Parameters => Enumerable.Empty<string>();


        public override string Description => "Configures the appsettings.json of a project";


        public override bool RequiresProfile => true;


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal SettingsCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public SettingsCommand(IAppSettingsManager appSettingsManager, IWizard<SettingsOptions> wizard)
        {
            this.wizard = wizard;
            this.appSettingsManager = appSettingsManager;
        }


        public override async Task Execute(ToolProfile? profile, string? action)
        {
            if (StopProcessing)
            {
                return;
            }

            if (profile is null)
            {
                LogError("No active profile.");
                return;
            }

            var options = await wizard.Run();
            switch (options.SettingToChange)
            {
                case SettingsOptions.ConnectionStringSetting:
                    await ConfigureConnectionString(profile);
                    break;
                case SettingsOptions.ConfigurationKeysSetting:
                    await ConfigureKeys(profile);
                    break;
                case SettingsOptions.CmsHeadlessSetting:
                    await ConfigureHeadlessOptions(profile);
                    break;
                default:
                    LogError("Invalid selection.");
                    return;
            }
        }


        public override async Task PostExecute(ToolProfile? profile, string? action)
        {
            AnsiConsole.WriteLine();

            await base.PostExecute(profile, action);
        }


        private async Task ConfigureHeadlessOptions(ToolProfile profile)
        {
            if (StopProcessing)
            {
                return;
            }

            bool updateHeadless = true;
            var headlessConfig = await appSettingsManager.GetCmsHeadlessConfiguration(profile);
            var configKeys = headlessConfig.GetType().GetProperties().Where(p => !p.Name.Equals(nameof(CmsHeadlessConfiguration.Caching)));
            var cachingKeys = headlessConfig.Caching.GetType().GetProperties();
            while (updateHeadless)
            {
                if (StopProcessing)
                {
                    return;
                }

                var propToUpdate = AnsiConsole.Prompt(new SelectionPrompt<PropertyInfo>()
                .Title($"Set which [{Constants.PROMPT_COLOR}]key[/]?")
                .PageSize(10)
                .UseConverter(v =>
                {
                    object? value = configKeys.Contains(v) ? v.GetValue(headlessConfig) : v.GetValue(headlessConfig.Caching);
                    string name = v.Name;
                    var displayAttr = v.GetCustomAttribute<DisplayAttribute>();
                    if (displayAttr?.Name is not null)
                    {
                        name = displayAttr.Name;
                    }

                    return $"{name} {(value is not null ? $"[{Constants.SUCCESS_COLOR}]({value})[/]" : "")}";
                })
                .MoreChoicesText("Scroll for more...")
                .AddChoices(configKeys.Union(cachingKeys)));

                bool success = await TryUpdateHeadlessOption(headlessConfig, propToUpdate, profile);
                updateHeadless = success &&
                    AnsiConsole.Prompt(new ConfirmationPrompt($"Update another [{Constants.PROMPT_COLOR}]headless key[/]?")
                    {
                        DefaultValue = true
                    });
            }
        }


        private async Task ConfigureConnectionString(ToolProfile profile)
        {
            if (StopProcessing)
            {
                return;
            }

            string connStringName = "CMSConnectionString";
            string? connString = await appSettingsManager.GetConnectionString(profile, connStringName);
            if (connString is null)
            {
                LogError($"Unable to load connection string.");
                return;
            }

            AnsiConsole.Write(new Markup($"[{Constants.PROMPT_COLOR} underline]{connStringName}[/]\n{connString}\n\n").Centered());

            string newConnString = AnsiConsole.Prompt(new TextPrompt<string>($"Enter new [{Constants.PROMPT_COLOR}]connection string[/]:"));
            await appSettingsManager.SetConnectionString(profile, connStringName, newConnString);

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Connection string updated![/]\n");
        }


        private async Task ConfigureKeys(ToolProfile profile)
        {
            if (StopProcessing)
            {
                return;
            }

            bool updateSettings = true;
            var keys = await appSettingsManager.GetConfigurationKeys(profile);
            while (updateSettings)
            {
                var updatedKey = GetNewSettingsKey(keys);
                if (updatedKey?.ActualValue is null)
                {
                    LogError($"Failed to set new value for key {updatedKey?.KeyName}");
                    return;
                }

                await appSettingsManager.SetKeyValue(profile, updatedKey.KeyName, updatedKey.ActualValue);
                AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Updated the {updatedKey.KeyName} key![/]\n");

                updateSettings = AnsiConsole.Prompt(new ConfirmationPrompt($"Update another [{Constants.PROMPT_COLOR}]configuration key[/]?")
                {
                    DefaultValue = true
                });
            }
        }


        private ConfigurationKey? GetNewSettingsKey(IEnumerable<ConfigurationKey> keys)
        {
            var keyToUpdate = AnsiConsole.Prompt(new SelectionPrompt<ConfigurationKey>()
                .Title($"Set which [{Constants.PROMPT_COLOR}]key[/]?")
                .PageSize(10)
                .UseConverter(v => $"{v.KeyName}{(v.ActualValue is not null ?
                    $" [{Constants.SUCCESS_COLOR}]({Truncate(v.ActualValue.ToString(), 12)})[/]" : string.Empty)}")
                .MoreChoicesText("Scroll for more...")
                .AddChoices(keys));

            var header = new StringBuilder($"\n[{Constants.PROMPT_COLOR} underline]{keyToUpdate.KeyName}[/]");
            if (keyToUpdate.ActualValue is not null)
            {
                header.AppendLine($"\nValue: {keyToUpdate.ActualValue}");
            }

            header.AppendLine($"\n{keyToUpdate.Description}\n");
            AnsiConsole.Write(new Markup(header.ToString()).Centered());

            string newValue = AnsiConsole.Prompt(new TextPrompt<string>($"New value [{Constants.EMPHASIS_COLOR}]({keyToUpdate.ValueType.Name.ToLower()})[/]:"));
            object converted = Convert.ChangeType(newValue, keyToUpdate.ValueType);
            if (converted is null)
            {
                LogError($"The key value cannot be cast into type {keyToUpdate.ValueType.Name}");
                return null;
            }

            keyToUpdate.ActualValue = converted;

            return keyToUpdate;
        }


        private async Task<bool> TryUpdateHeadlessOption(CmsHeadlessConfiguration headlessConfiguration, PropertyInfo propToUpdate, ToolProfile profile)
        {
            bool isCachingKey = headlessConfiguration.Caching.GetType().GetProperties().Contains(propToUpdate);
            object? value = isCachingKey ? propToUpdate.GetValue(headlessConfiguration.Caching) : propToUpdate.GetValue(headlessConfiguration);
            var displayAttr = propToUpdate.GetCustomAttribute<DisplayAttribute>();
            var header = new StringBuilder($"\n[{Constants.PROMPT_COLOR} underline]{displayAttr?.Name ?? propToUpdate.Name}[/]");
            if (value is not null)
            {
                header.AppendLine($"\nValue: {value}");
            }

            if (displayAttr?.Description is not null)
            {
                header.AppendLine($"\n{displayAttr?.Description}\n");
            }

            AnsiConsole.Write(new Markup(header.ToString()).Centered());

            string newValue = AnsiConsole.Prompt(new TextPrompt<string>($"New value [{Constants.EMPHASIS_COLOR}]({propToUpdate.PropertyType.Name.ToLower()})[/]:"));
            object converted = Convert.ChangeType(newValue, propToUpdate.PropertyType);
            if (converted is null)
            {
                LogError($"The key value cannot be cast into type {propToUpdate.PropertyType.Name}");
                return false;
            }

            if (isCachingKey)
            {
                propToUpdate.SetValue(headlessConfiguration.Caching, converted);
            }
            else
            {
                propToUpdate.SetValue(headlessConfiguration, converted);
            }

            await appSettingsManager.SetCmsHeadlessConfiguration(profile, headlessConfiguration);
            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Updated the {displayAttr?.Name ?? propToUpdate.Name} key![/]\n");

            return true;
        }


        private string? Truncate(string? value, int maxLength, string truncationSuffix = "...") => value?.Length > maxLength
                ? value[..maxLength] + truncationSuffix
                : value;
    }
}
