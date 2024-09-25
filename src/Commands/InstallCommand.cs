using Spectre.Console;

using System.Diagnostics;

using Xperience.Manager.Configuration;
using Xperience.Manager.Helpers;
using Xperience.Manager.Options;
using Xperience.Manager.Services;
using Xperience.Manager.Wizards;

namespace Xperience.Manager.Commands
{
    /// <summary>
    /// A command which installs new Xperience by Kentico project files and database in the current directory.
    /// </summary>
    public class InstallCommand : AbstractCommand
    {
        private const string DATABASE = "db";
        private readonly ToolProfile newInstallationProfile = new();
        private readonly IShellRunner shellRunner;
        private readonly IConfigManager configManager;
        private readonly IScriptBuilder scriptBuilder;
        private readonly IWizard<InstallProjectOptions> projectWizard;
        private readonly IWizard<InstallDatabaseOptions> dbWizard;


        public override IEnumerable<string> Keywords => ["i", "install"];


        public override IEnumerable<string> Parameters => [DATABASE];


        public override string Description => "Installs a new XbK instance. The 'db' parameter installs only a database";


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal InstallCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public InstallCommand(
            IShellRunner shellRunner,
            IScriptBuilder scriptBuilder,
            IWizard<InstallProjectOptions> projectWizard,
            IWizard<InstallDatabaseOptions> dbWizard,
            IConfigManager configManager)
        {
            this.dbWizard = dbWizard;
            this.projectWizard = projectWizard;
            this.shellRunner = shellRunner;
            this.configManager = configManager;
            this.scriptBuilder = scriptBuilder;
        }


        public override async Task Execute(ToolProfile? profile, string? action)
        {
            if (StopProcessing)
            {
                return;
            }

            // Override default values of InstallDatabaseOptions and InstallProjectOptions with values from config file
            dbWizard.Options = await configManager.GetDefaultInstallDatabaseOptions();
            projectWizard.Options = await configManager.GetDefaultInstallProjectOptions();

            // Only install database if "db" argument is passed
            InstallDatabaseOptions? dbOptions = null;
            if (!string.IsNullOrEmpty(action) && action.Equals(DATABASE))
            {
                dbOptions = await dbWizard.Run(InstallDatabaseWizard.SKIP_EXISTINGDB_STEP);
                await InstallDatabaseTool();
                await CreateDatabase(dbOptions, true);

                return;
            }

            var projectOptions = await projectWizard.Run();
            if (!IsAdminTemplate(projectOptions))
            {
                dbOptions = await dbWizard.Run();
            }

            newInstallationProfile.ProjectName = projectOptions.ProjectName;
            newInstallationProfile.WorkingDirectory = $"{projectOptions.InstallRootPath}\\{projectOptions.ProjectName}";

            AnsiConsole.WriteLine();
            await CreateWorkingDirectory();
            await InstallTemplate(projectOptions);
            await CreateProjectFiles(projectOptions);

            // Admin boilerplate project doesn't require database install or profile
            if (!IsAdminTemplate(projectOptions) && dbOptions is not null)
            {
                await CreateDatabase(dbOptions, false);
                await configManager.AddProfile(newInstallationProfile);

                // Select new profile
                AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Setting profile to '{newInstallationProfile.ProjectName}'...[/]");
                await configManager.SetCurrentProfile(newInstallationProfile);
            }
        }


        public override async Task PostExecute(ToolProfile? profile, string? action)
        {
            if (!Errors.Any())
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Install complete![/]\n");
            }

            await base.PostExecute(profile, action);
        }


        private async Task CreateWorkingDirectory()
        {
            if (StopProcessing)
            {
                return;
            }

            if (string.IsNullOrEmpty(newInstallationProfile.WorkingDirectory))
            {
                LogError("Unable to load working directory.");
                return;
            }

            string mkdirScript = scriptBuilder.SetScript(ScriptType.CreateDirectory)
                .AppendDirectory(newInstallationProfile.WorkingDirectory)
                .Build();
            await shellRunner.Execute(new(mkdirScript) { ErrorHandler = ErrorDataReceived }).WaitForExitAsync();
        }


        private async Task CreateDatabase(InstallDatabaseOptions options, bool isDatabaseOnly)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Running database creation script...[/]");

            string databaseScript = scriptBuilder.SetScript(ScriptType.DatabaseInstall)
                .WithPlaceholders(options)
                .Build();
            // Database-only install requires removal of "dotnet" from the script to run global tool
            if (isDatabaseOnly)
            {
                databaseScript = databaseScript.Replace("dotnet", "");
            }

            await shellRunner.Execute(new(databaseScript)
            {
                ErrorHandler = ErrorDataReceived,
                WorkingDirectory = newInstallationProfile.WorkingDirectory
            }).WaitForExitAsync();
        }


        private async Task CreateProjectFiles(InstallProjectOptions options)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Running project creation script...[/]");

            string installScript = scriptBuilder.SetScript(ScriptType.ProjectInstall)
                .WithPlaceholders(options)
                .AppendCloud(options.UseCloud)
                .Build();

            // Admin boilerplate script doesn't require input
            bool keepOpen = !IsAdminTemplate(options);
            await shellRunner.Execute(new(installScript)
            {
                KeepOpen = keepOpen,
                WorkingDirectory = newInstallationProfile.WorkingDirectory,
                ErrorHandler = ErrorDataReceived,
                OutputHandler = (o, e) =>
                {
                    var proc = o as Process;
                    if (e.Data?.Contains("Do you want to run this action", StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        // Restore packages when prompted
                        proc?.StandardInput.WriteLine("Y");
                        proc?.StandardInput.Close();
                    }
                }
            }).WaitForExitAsync();
        }

        private async Task InstallDatabaseTool()
        {
            if (StopProcessing)
            {
                return;
            }

            // Get desired database tool version
            var versions = await NuGetVersionHelper.GetPackageVersions(Constants.DATABASE_TOOL);
            var filtered = versions.Where(v => !v.IsPrerelease && !v.IsLegacyVersion && v.Major >= 25)
                .Select(v => v.Version)
                .OrderByDescending(v => v);
            var toolVersion = AnsiConsole.Prompt(new SelectionPrompt<Version>()
                    .Title($"Which [{Constants.PROMPT_COLOR}]version[/]?")
                    .PageSize(10)
                    .UseConverter(v => $"{v.Major}.{v.Minor}.{v.Build}")
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(filtered));

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Uninstalling previous database tool...[/]");

            string uninstallScript = scriptBuilder.SetScript(ScriptType.UninstallDatabaseTool).Build();
            // Don't use base error handler for uninstall script as it throws when no tool is installed
            // Just skip uninstall step in case of error and try to continue
            await shellRunner.Execute(new(uninstallScript)).WaitForExitAsync();

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Installing database tool version {toolVersion}...[/]");

            string installScript = scriptBuilder.SetScript(ScriptType.InstallDatabaseTool)
                .AppendVersion(toolVersion)
                .Build();
            await shellRunner.Execute(new(installScript) { ErrorHandler = ErrorDataReceived }).WaitForExitAsync();
        }


        private async Task InstallTemplate(InstallProjectOptions options)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Uninstalling previous template version...[/]");

            string uninstallScript = scriptBuilder.SetScript(ScriptType.TemplateUninstall).Build();
            // Don't use base error handler for uninstall script as it throws when no templates are installed
            // Just skip uninstall step in case of error and try to continue
            await shellRunner.Execute(new(uninstallScript)).WaitForExitAsync();

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Installing template version {options.Version}...[/]");

            string installScript = scriptBuilder.SetScript(ScriptType.TemplateInstall)
                .WithPlaceholders(options)
                .AppendVersion(options.Version)
                .Build();
            await shellRunner.Execute(new(installScript) { ErrorHandler = ErrorDataReceived }).WaitForExitAsync();
        }


        private static bool IsAdminTemplate(InstallProjectOptions options) =>
            options?.Template.Equals(Constants.TEMPLATE_ADMIN, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
