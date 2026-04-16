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
            Steps.Add(new Step<string>(new()
            {
                Prompt = new SelectionPrompt<string>()
                    .Title($"Database [{Constants.PROMPT_COLOR}]authentication method[/]?")
                    .AddChoices(InstallDatabaseOptions.AUTHENTICATION_INTEGRATED, InstallDatabaseOptions.AUTHENTICATION_USER),
                ValueReceiver = (v) => Options.DatabaseAuthenticationType = v
            }));

            var databaseUserNamePrompt = new TextPrompt<string>($"Database [{Constants.PROMPT_COLOR}]user name[/]:");
            if (!string.IsNullOrEmpty(Options.DatabaseUserName))
            {
                databaseUserNamePrompt.DefaultValue(Options.DatabaseUserName);
            }
            Steps.Add(new Step<string>(new()
            {
                Prompt = databaseUserNamePrompt,
                ValueReceiver = (v) => Options.DatabaseUserName = v,
                SkipChecker = () => Options.DatabaseAuthenticationType?
                    .Equals(InstallDatabaseOptions.AUTHENTICATION_INTEGRATED, StringComparison.InvariantCultureIgnoreCase) ?? false
            }));

            var databasePasswordPrompt = new TextPrompt<string>($"Database [{Constants.PROMPT_COLOR}]password[/]:");
            if (!string.IsNullOrEmpty(Options.DatabasePassword))
            {
                databasePasswordPrompt.DefaultValue(Options.DatabasePassword);
            }
            Steps.Add(new Step<string>(new()
            {
                Prompt = databasePasswordPrompt,
                ValueReceiver = (v) => Options.DatabasePassword = v,
                SkipChecker = () => Options.DatabaseAuthenticationType?
                    .Equals(InstallDatabaseOptions.AUTHENTICATION_INTEGRATED, StringComparison.InvariantCultureIgnoreCase) ?? false
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
