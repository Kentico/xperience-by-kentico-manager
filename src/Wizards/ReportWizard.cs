using Spectre.Console;

using Xperience.Manager.Helpers;
using Xperience.Manager.Options;
using Xperience.Manager.Steps;

namespace Xperience.Manager.Wizards
{
    /// <summary>
    /// A wizard which generates an <see cref="ReportOptions"/> for generating reports. The 
    /// <see cref="AbstractWizard{TOptions}.Run(string[])"/> method should be passed the working directory of the project.
    /// </summary>
    public class ReportWizard : AbstractWizard<ReportOptions>
    {
        public override Task InitSteps(params string[] args)
        {
            if (args.Length != 1)
            {
                throw new InvalidOperationException($"{nameof(ReportWizard)} requires exactly 1 argument.");
            }

            string workingDirectory = args[0];
            if (AssetHelper.DefaultDirectoryExists(workingDirectory))
            {
                return Task.CompletedTask;
            }

            bool isUsingCustomDir = false;
            Steps.Add(new Step<bool>(new()
            {
                Prompt = new ConfirmationPrompt($"Default asset directory not found. Are you using a [{Constants.PROMPT_COLOR}]custom" +
                    $" asset directory[/]?")
                {
                    DefaultValue = true
                },
                ValueReceiver = (v) => isUsingCustomDir = v
            }));
            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Custom directory [{Constants.PROMPT_COLOR}]name[/]:"),
                ValueReceiver = (v) => Options.CustomAssetDirectoryName = v,
                SkipChecker = () => !isUsingCustomDir
            }));

            return Task.CompletedTask;
        }
    }
}
