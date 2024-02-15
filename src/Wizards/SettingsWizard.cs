using Spectre.Console;

using Xperience.Manager.Options;
using Xperience.Manager.Steps;

namespace Xperience.Manager.Wizards
{
    /// <summary>
    /// A wizard which generates an <see cref="SettingsOptions"/> for configuring appsettings.json.
    /// </summary>
    public class SettingsWizard : AbstractWizard<SettingsOptions>
    {
        public override Task InitSteps()
        {
            Steps.Add(new Step<string>(new()
            {
                Prompt = new SelectionPrompt<string>()
                    .Title($"What [{Constants.PROMPT_COLOR}]settings[/] do you want to change?")
                    .PageSize(10)
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(
                        SettingsOptions.ConnectionStringSetting,
                        SettingsOptions.ConfigurationKeysSetting,
                        SettingsOptions.CmsHeadlessSetting
                    ),
                ValueReceiver = (v) => Options.SettingToChange = v
            }));

            return Task.CompletedTask;
        }
    }
}
