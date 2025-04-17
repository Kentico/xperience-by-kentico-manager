using Spectre.Console;

using Xperience.Manager.Configuration;
using Xperience.Manager.Options;
using Xperience.Manager.Services;

namespace Xperience.Manager.Commands
{
    /// <summary>
    /// A command which deletes an Xperience by Kentico project.
    /// </summary>
    public class DeleteCommand : AbstractCommand
    {
        private bool deleteConfirmed;
        private readonly IShellRunner shellRunner;
        private readonly IScriptBuilder scriptBuilder;
        private readonly IConfigManager configManager;
        private readonly IAppSettingsManager appSettingsManager;


        public override IEnumerable<string> Keywords => ["d", "delete"];


        public override IEnumerable<string> Parameters => [];


        public override string Description => "Deletes a project and its database";


        public override bool RequiresProfile => true;


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal DeleteCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public DeleteCommand(
            IShellRunner shellRunner,
            IScriptBuilder scriptBuilder,
            IConfigManager configManager,
            IAppSettingsManager appSettingsManager)
        {
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
            this.configManager = configManager;
            this.appSettingsManager = appSettingsManager;
        }


        public override async Task Execute(ToolProfile? profile, string? action)
        {
            deleteConfirmed = AnsiConsole.Confirm($"This will [{Constants.ERROR_COLOR}]delete[/] the current profile's physical folder " +
                $"and database!\nDo you want to continue?", false);
            if (!deleteConfirmed)
            {
                return;
            }

            AnsiConsole.WriteLine();
            await DropDatabase(profile);
            await UninstallFiles(profile);
            if (!StopProcessing)
            {
                await configManager.RemoveProfile(profile);
            }
        }


        public override async Task PostExecute(ToolProfile? profile, string? action)
        {
            if (!deleteConfirmed)
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Delete cancelled[/]\n");
            }
            else if (Errors.Count == 0)
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Delete complete![/]\n");
            }

            await base.PostExecute(profile, action);
        }


        private async Task DropDatabase(ToolProfile? profile)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Deleting database...[/]");

            string? connString = await appSettingsManager.GetConnectionString(profile, "CMSConnectionString");
            if (connString is null)
            {
                LogError("Couldn't load connection string.");

                return;
            }

            // Find "Initial Catalog" in connection string
            IEnumerable<string> parts = [.. connString.Split(';')];
            string? initialCatalogPart = parts.FirstOrDefault(p => p.StartsWith("initial catalog", StringComparison.CurrentCultureIgnoreCase));
            if (initialCatalogPart is null)
            {
                LogError("Couldn't find database name.");

                return;
            }

            // Remove "Initial Catalog" from connection string, or trying to delete will throw "in use" error
            parts = parts.Where(p => !p.Equals(initialCatalogPart, StringComparison.OrdinalIgnoreCase));
            connString = string.Join(';', parts);
            string databaseName = initialCatalogPart.Split('=')[1].Trim();

            var options = new RunSqlOptions()
            {
                SqlQuery = $"DROP DATABASE {databaseName}",
                ConnString = connString
            };
            string dbScript = scriptBuilder.SetScript(ScriptType.ExecuteSql).WithPlaceholders(options).Build();
            await shellRunner.Execute(new(dbScript)
            {
                ErrorHandler = ErrorDataReceived
            }).WaitForExitAsync();
        }


        private async Task UninstallFiles(ToolProfile? profile)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Deleting local files...[/]");

            string uninstallScript = scriptBuilder.SetScript(ScriptType.DeleteDirectory).WithPlaceholders(profile).Build();
            await shellRunner.Execute(new(uninstallScript)
            {
                ErrorHandler = ErrorDataReceived
            }).WaitForExitAsync();
        }
    }
}
