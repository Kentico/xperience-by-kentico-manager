using Spectre.Console;

using Xperience.Xman.Options;
using Xperience.Xman.Steps;

namespace Xperience.Xman.Wizards
{
    /// <summary>
    /// A wizard which generates a <see cref="RepositoryConfiguration"/> for customizing Continuous Deployment configuration files.
    /// </summary>
    public class RepositoryConfigurationWizard : AbstractWizard<RepositoryConfiguration>
    {
        private bool changeIncluded;
        private bool changeExcluded;
        private readonly IEnumerable<string> restoreModes = new string[]
        {
            "Create",
            "CreateUpdate",
            "Full"
        };


        public override Task InitSteps()
        {
            Steps.Add(new Step<string>(new()
            {
                Prompt = new SelectionPrompt<string>()
                    .Title($"Which [{Constants.PROMPT_COLOR}]RestoreMode[/]? [{Constants.SUCCESS_COLOR}]({Options.RestoreMode})[/]")
                    .PageSize(10)
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(restoreModes),
                ValueReceiver = (v) => Options.RestoreMode = v
            }));

            Steps.Add(new Step<bool>(new()
            {
                Prompt = new ConfirmationPrompt($"[{Constants.PROMPT_COLOR}]Included[/] object types: {string.Join(";", Options.IncludedObjectTypes ?? Enumerable.Empty<string>())}\nWould you like to change them?")
                {
                    DefaultValue = false
                },
                ValueReceiver = (v) => changeIncluded = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Enter new [{Constants.PROMPT_COLOR}]included[/] object types separated by semi-colon:")
                    .AllowEmpty(),
                ValueReceiver = (v) =>
                {
                    string[] types = v.Split(';');
                    var list = new List<string>(types.Where(t => !string.IsNullOrEmpty(t)));
                    Options.IncludedObjectTypes = list;
                },
                SkipChecker = () => !changeIncluded
            }));

            Steps.Add(new Step<bool>(new()
            {
                Prompt = new ConfirmationPrompt($"[{Constants.PROMPT_COLOR}]Excluded[/] object types: {string.Join(";", Options.ExcludedObjectTypes ?? Enumerable.Empty<string>())}\nWould you like to change them?")
                {
                    DefaultValue = false
                },
                ValueReceiver = (v) => changeExcluded = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Enter new [{Constants.PROMPT_COLOR}]excluded[/] object types separated by semi-colon:")
                    .AllowEmpty(),
                ValueReceiver = (v) =>
                {
                    string[] types = v.Split(';');
                    var list = new List<string>(types.Where(t => !string.IsNullOrEmpty(t)));
                    Options.ExcludedObjectTypes = list;
                },
                SkipChecker = () => !changeExcluded
            }));

            return Task.CompletedTask;
        }
    }
}
