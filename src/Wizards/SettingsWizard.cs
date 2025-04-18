using Spectre.Console;

using Xperience.Manager.Options;
using Xperience.Manager.Steps;

namespace Xperience.Manager.Wizards
{
    /// <summary>
    /// A wizard which generates an <see cref="SettingsOptions"/> for configuring application settings. The 
    /// <see cref="AbstractWizard{TOptions}.Run(string[])"/> method should be passed a list of available settings files.
    /// </summary>
    public class SettingsWizard : AbstractWizard<SettingsOptions>
    {
        public override Task InitSteps(params string[] args)
        {
            // List available appsettings files for selection
            if (args.Length > 1)
            {
                Array.Sort(args, (a, b) => a.Length.CompareTo(b.Length));
                Steps.Add(new Step<string>(new()
                {
                    Prompt = new SelectionPrompt<string>()
                    .Title($"Which [{Constants.PROMPT_COLOR}]file[/] do you want to modify?")
                    .PageSize(10)
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(args),
                    ValueReceiver = (v) => Options.AppSettingsFileName = v
                }));
            }

            Steps.Add(new Step<string>(new()
            {
                Prompt = new SelectionPrompt<string>()
                    .Title($"What [{Constants.PROMPT_COLOR}]settings[/] do you want to change?")
                    .PageSize(10)
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(
                        SettingsOptions.ConnectionStringSetting,
                        SettingsOptions.UngroupedKeySetting,
                        SettingsOptions.CmsHeadlessSetting,
                        SettingsOptions.AzureStorageSetting
                    ),
                ValueReceiver = (v) => Options.SettingToChange = v
            }));

            return Task.CompletedTask;
        }
    }
}
