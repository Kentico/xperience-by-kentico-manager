using Spectre.Console;

using Xperience.Manager.Configuration;
using Xperience.Manager.Options;
using Xperience.Manager.Services;
using Xperience.Manager.Wizards;

namespace Xperience.Manager.Commands
{
    /// <summary>
    /// A command which updates the NuGet packages and database of the Xperience by Kentico project in
    /// the current directory.
    /// </summary>
    public class UpdateCommand : AbstractCommand
    {
        private readonly IShellRunner shellRunner;
        private readonly IScriptBuilder scriptBuilder;
        private readonly IWizard<UpdateOptions> wizard;
        private readonly IEnumerable<string> packageNames =
        [
            "kentico.xperience.admin",
            "kentico.xperience.azurestorage",
            "kentico.xperience.cloud",
            "kentico.xperience.graphql",
            "kentico.xperience.imageprocessing",
            "kentico.xperience.webapp"
        ];


        public override IEnumerable<string> Keywords => ["u", "update"];


        public override IEnumerable<string> Parameters => Enumerable.Empty<string>();


        public override string Description => "Updates a project's NuGet packages and database version";


        public override bool RequiresProfile => true;


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal UpdateCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public UpdateCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder, IWizard<UpdateOptions> wizard)
        {
            this.wizard = wizard;
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
        }


        public override async Task Execute(ToolProfile? profile, string? action)
        {
            if (StopProcessing)
            {
                return;
            }

            var options = await wizard.Run();

            await UpdatePackages(options, profile);
            await BuildProject(profile);
            await UpdateDatabase(profile);
        }


        public override async Task PostExecute(ToolProfile? profile, string? action)
        {
            if (!Errors.Any())
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Update complete![/]\n");
            }

            await base.PostExecute(profile, action);
        }


        private async Task BuildProject(ToolProfile? profile)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Attempting to build the project...[/]");

            string buildScript = scriptBuilder.SetScript(ScriptType.BuildProject).Build();
            await shellRunner.Execute(new(buildScript)
            {
                ErrorHandler = ErrorDataReceived,
                WorkingDirectory = profile?.WorkingDirectory
            }).WaitForExitAsync();
        }


        private async Task UpdatePackages(UpdateOptions options, ToolProfile? profile)
        {
            foreach (string package in packageNames)
            {
                if (StopProcessing)
                {
                    return;
                }

                AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Updating {package} to version {options.Version}...[/]");

                options.PackageName = package;
                string packageScript = scriptBuilder.SetScript(ScriptType.PackageUpdate)
                    .WithPlaceholders(options)
                    .AppendVersion(options.Version)
                    .Build();
                await shellRunner.Execute(new(packageScript)
                {
                    ErrorHandler = ErrorDataReceived,
                    WorkingDirectory = profile?.WorkingDirectory
                }).WaitForExitAsync();
            }
        }


        private async Task UpdateDatabase(ToolProfile? profile)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Updating database...[/]");
            string dbScript = scriptBuilder.SetScript(ScriptType.DatabaseUpdate).Build();
            await shellRunner.Execute(new(dbScript)
            {
                ErrorHandler = ErrorDataReceived,
                WorkingDirectory = profile?.WorkingDirectory
            }).WaitForExitAsync();
        }
    }
}
