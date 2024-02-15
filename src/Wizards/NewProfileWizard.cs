using Spectre.Console;

using Xperience.Manager.Options;
using Xperience.Manager.Steps;

namespace Xperience.Manager.Wizards
{
    /// <summary>
    /// A wizard which generates a <see cref="NewProfileOptions"/> for adding new profiles.
    /// </summary>
    public class NewProfileWizard : AbstractWizard<NewProfileOptions>
    {
        public override Task InitSteps()
        {
            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Give your profile a [{Constants.PROMPT_COLOR}]name[/]:"),
                ValueReceiver = (v) => Options.Name = v,
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Enter the [{Constants.PROMPT_COLOR}]full path[/] of the folder containing your Xperience project:"),
                ValueReceiver = (v) => Options.WorkingDirectory = v,
            }));

            return Task.CompletedTask;
        }
    }
}
