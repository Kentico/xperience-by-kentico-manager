using Spectre.Console;

using Xperience.Manager.Configuration;
using Xperience.Manager.Options;
using Xperience.Manager.Services;
using Xperience.Manager.Wizards;

namespace Xperience.Manager.Commands
{
    /// <summary>
    /// A command which allows re-signing of macros.
    /// </summary>
    public class MacroCommand : AbstractCommand
    {
        private readonly IShellRunner shellRunner;
        private readonly IScriptBuilder scriptBuilder;
        private readonly IWizard<MacroOptions> wizard;


        public override IEnumerable<string> Keywords => ["m", "macros"];


        public override IEnumerable<string> Parameters => Enumerable.Empty<string>();


        public override string Description => "Re-signs macro signatures";


        public override bool RequiresProfile => true;


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal MacroCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public MacroCommand(IWizard<MacroOptions> wizard, IShellRunner shellRunner, IScriptBuilder scriptBuilder)
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
            AnsiConsole.WriteLine();

            await AnsiConsole.Progress()
                .Columns(
                [
                    new SpinnerColumn(),
                    new ElapsedTimeColumn(),
                    new TaskDescriptionColumn()
                ])
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask($"[{Constants.EMPHASIS_COLOR}]Re-signing macros...[/]");
                    await ResignMacros(task, profile, options);
                });
        }


        public override async Task PostExecute(ToolProfile? profile, string? action)
        {
            if (!Errors.Any())
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Macros re-signed![/]\n");
            }

            await base.PostExecute(profile, action);
        }


        private async Task ResignMacros(ProgressTask task, ToolProfile? profile, MacroOptions options)
        {
            if (StopProcessing)
            {
                return;
            }

            string originalDescription = task.Description;
            string macroScript = scriptBuilder.SetScript(ScriptType.ResignMacros)
                .AppendSignAll(options.SignAll, options.UserName)
                .AppendSalts(options.OldSalt, options.NewSalt)
                .Build();
            await shellRunner.Execute(new(macroScript)
            {
                ErrorHandler = ErrorDataReceived,
                WorkingDirectory = profile?.WorkingDirectory,
                OutputHandler = (o, e) =>
                {
                    if (e.Data?.Contains("Processing", StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        // Message is something like "Processing 'Class' objects"
                        task.Description = e.Data;
                    }
                }
            }).WaitForExitAsync();

            task.Description = originalDescription;
        }
    }
}
