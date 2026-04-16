using Spectre.Console;

using Xperience.Manager.Options;
using Xperience.Manager.Steps;

namespace Xperience.Manager.Wizards
{
    /// <summary>
    /// A wizard which generates an <see cref="InstallDatabaseOptions"/> for installing Xperience by Kentico databases.
    /// </summary>
    public class InstallDatabaseWizard : AbstractWizard<InstallDatabaseOptions>
    {
        public const string SKIP_EXISTINGDB_STEP = "skipexistingdbstep";
        private const string AUTHENTICATION_USER = "User";
        private const string AUTHENTICATION_INTEGRATED = "Integrated";


        public override Task InitSteps(params string[] args)
        {
            var serverPrompt = new TextPrompt<string>($"Enter the [{Constants.PROMPT_COLOR}]SQL server[/] name:");
            if (!string.IsNullOrEmpty(Options.ServerName))
            {
                serverPrompt.DefaultValue(Options.ServerName);
            }
            Steps.Add(new Step<string>(new()
            {
                Prompt = serverPrompt,
                ValueReceiver = (v) => Options.ServerName = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Enter the [{Constants.PROMPT_COLOR}]database[/] name:")
                    .AllowEmpty()
                    .DefaultValue(Options.DatabaseName),
                ValueReceiver = (v) => Options.DatabaseName = v
            }));

            // Integrated or user/pass database authentication
            string authenticationType = AUTHENTICATION_INTEGRATED;
            Steps.Add(new Step<string>(new()
            {
                Prompt = new SelectionPrompt<string>()
                    .Title($"Database [{Constants.PROMPT_COLOR}]authentication method[/]?")
                    .AddChoices(AUTHENTICATION_INTEGRATED, AUTHENTICATION_USER),
                ValueReceiver = (v) => authenticationType = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Database [{Constants.PROMPT_COLOR}]user name[/]:"),
                ValueReceiver = (v) => Options.DatabaseUserName = v,
                SkipChecker = () => authenticationType.Equals(AUTHENTICATION_INTEGRATED, StringComparison.InvariantCultureIgnoreCase)
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Database [{Constants.PROMPT_COLOR}]password[/]:"),
                ValueReceiver = (v) => Options.DatabasePassword = v,
                SkipChecker = () => authenticationType.Equals(AUTHENTICATION_INTEGRATED, StringComparison.InvariantCultureIgnoreCase)
            }));

            var useExistingPrompt = new ConfirmationPrompt($"Use [{Constants.PROMPT_COLOR}]existing[/] database?")
            {
                DefaultValue = Options.UseExistingDatabase
            };
            Steps.Add(new Step<bool>(new()
            {
                Prompt = useExistingPrompt,
                ValueReceiver = (v) => Options.UseExistingDatabase = v,
                SkipChecker = () => args.Contains(SKIP_EXISTINGDB_STEP)
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Enter the admin [{Constants.PROMPT_COLOR}]password[/]:")
                    .AllowEmpty()
                    .DefaultValue(Options.AdminPassword),
                ValueReceiver = (v) => Options.AdminPassword = v
            }));

            return Task.CompletedTask;
        }
    }
}
