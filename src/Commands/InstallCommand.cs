using Spectre.Console;

using System.Diagnostics;

using Xperience.Manager.Configuration;
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
        private readonly ToolProfile newInstallationProfile = new();
        private readonly IShellRunner shellRunner;
        private readonly IConfigManager configManager;
        private readonly IScriptBuilder scriptBuilder;
        private readonly IWizard<InstallOptions> wizard;


        public override IEnumerable<string> Keywords => ["i", "install"];


        public override IEnumerable<string> Parameters => Enumerable.Empty<string>();


        public override string Description => "Installs a new XbK instance";


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal InstallCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public InstallCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder, IWizard<InstallOptions> wizard, IConfigManager configManager)
        {
            this.wizard = wizard;
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

            // Override default values of InstallOptions with values from config file
            wizard.Options = await configManager.GetDefaultInstallOptions();
            var options = await wizard.Run();
            AnsiConsole.WriteLine();

            newInstallationProfile.ProjectName = options.ProjectName;
            newInstallationProfile.WorkingDirectory = $"{options.InstallRootPath}\\{options.ProjectName}";

            await CreateWorkingDirectory();
            await InstallTemplate(options);
            await CreateProjectFiles(options);

            // Admin boilerplate project doesn't require database install or profile
            if (!IsAdminTemplate(options))
            {
                await CreateDatabase(options);
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


        private async Task CreateDatabase(InstallOptions options)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Running database creation script...[/]");

            string databaseScript = scriptBuilder.SetScript(ScriptType.DatabaseInstall)
                .WithPlaceholders(options)
                .Build();
            await shellRunner.Execute(new(databaseScript)
            {
                ErrorHandler = ErrorDataReceived,
                WorkingDirectory = newInstallationProfile.WorkingDirectory
            }).WaitForExitAsync();
        }


        private async Task CreateProjectFiles(InstallOptions options)
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


        private async Task InstallTemplate(InstallOptions options)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Uninstalling previous template version...[/]");

            string uninstallScript = scriptBuilder.SetScript(ScriptType.TemplateUninstall).Build();
            // Don't use base error handler for uninstall script as it throws when no templates are installed
            // Just skip uninstall step in case of error and try to continue
            var uninstallCmd = shellRunner.Execute(new(uninstallScript));
            await uninstallCmd.WaitForExitAsync();

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Installing template version {options.Version}...[/]");

            string installScript = scriptBuilder.SetScript(ScriptType.TemplateInstall)
                .WithPlaceholders(options)
                .AppendVersion(options.Version)
                .Build();
            var installCmd = shellRunner.Execute(new(installScript) { ErrorHandler = ErrorDataReceived });
            await installCmd.WaitForExitAsync();
        }


        private bool IsAdminTemplate(InstallOptions options) => options?.Template.Equals(Constants.TEMPLATE_ADMIN, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
