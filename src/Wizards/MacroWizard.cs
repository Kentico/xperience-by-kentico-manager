using Spectre.Console;

using Xperience.Xman.Options;
using Xperience.Xman.Steps;

namespace Xperience.Xman.Wizards
{
    /// <summary>
    /// A wizard which generates an <see cref="MacroOptions"/> for re-signing macros.
    /// </summary>
    public class MacroWizard : AbstractWizard<MacroOptions>
    {
        public override Task InitSteps()
        {
            Steps.Add(new Step<bool>(new()
            {
                Prompt = new ConfirmationPrompt($"[{Constants.PROMPT_COLOR}]Sign all[/] macros?")
                {
                    DefaultValue = true
                },
                ValueReceiver = (v) => Options.SignAll = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"[{Constants.PROMPT_COLOR}]Username[/]:"),
                ValueReceiver = (v) => Options.UserName = v,
                SkipChecker = () => !Options.SignAll
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"[{Constants.PROMPT_COLOR}]Old[/] salt:"),
                ValueReceiver = (v) => Options.OldSalt = v,
                SkipChecker = () => Options.SignAll
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"[{Constants.PROMPT_COLOR}]New[/] salt? Leave empty to use the salt in appsettings:")
                    .AllowEmpty(),
                ValueReceiver = (v) => Options.NewSalt = v,
            }));

            return Task.CompletedTask;
        }
    }
}
