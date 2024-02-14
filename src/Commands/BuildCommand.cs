using Spectre.Console;

using Xperience.Xman.Configuration;
using Xperience.Xman.Services;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// A command which builds an Xperience by Kentico project.
    /// </summary>
    public class BuildCommand : AbstractCommand
    {
        private readonly IShellRunner shellRunner;
        private readonly IScriptBuilder scriptBuilder;


        public override IEnumerable<string> Keywords => ["b", "build"];


        public override IEnumerable<string> Parameters => Enumerable.Empty<string>();


        public override string Description => "Builds a project";


        public override bool RequiresProfile => true;


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal BuildCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public BuildCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder)
        {
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
        }


        public override async Task Execute(ToolProfile? profile, string? action) => await BuildProject(profile);


        public override async Task PostExecute(ToolProfile? profile, string? action)
        {
            if (!Errors.Any())
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Build complete![/]\n");
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
    }
}
