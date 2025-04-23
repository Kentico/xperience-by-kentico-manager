using Spectre.Console;

using System.Diagnostics;

using Xperience.Manager.Configuration;
using Xperience.Manager.Services;

namespace Xperience.Manager.Commands
{
    /// <summary>
    /// A command which stores or restores Continuous Integration data.
    /// </summary>
    public class ContinuousIntegrationCommand : AbstractCommand
    {
        private const string STORE = "store";
        private const string RESTORE = "restore";
        private readonly IShellRunner shellRunner;
        private readonly IScriptBuilder scriptBuilder;


        public override IEnumerable<string> Keywords => ["ci"];


        public override IEnumerable<string> Parameters => [STORE, RESTORE];


        public override string Description => "Stores or restores CI data";


        public override bool RequiresProfile => true;


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal ContinuousIntegrationCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public ContinuousIntegrationCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder)
        {
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
        }


        public override async Task PreExecute(ToolProfile? profile, string? action)
        {
            if (string.IsNullOrEmpty(action) || !Parameters.Any(p => p.Equals(action, StringComparison.OrdinalIgnoreCase)))
            {
                LogError($"Must provide one parameter from '{string.Join(", ", Parameters)}'");

                return;
            }

            await base.PreExecute(profile, action);
        }


        public override async Task Execute(ToolProfile? profile, string? action)
        {
            if (StopProcessing)
            {
                return;
            }

            if (action?.Equals(STORE, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                await AnsiConsole.Progress()
                    .Columns(
                    [
                        new SpinnerColumn(),
                        new ElapsedTimeColumn(),
                        new TaskDescriptionColumn(),
                        new ProgressBarColumn(),
                        new PercentageColumn()
                    ])
                    .StartAsync(async ctx =>
                    {
                        var task = ctx.AddTask($"[{Constants.EMPHASIS_COLOR}]Running the CI store script[/]");
                        await StoreFiles(task, profile);
                    });
            }
            else if (action?.Equals(RESTORE, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                await AnsiConsole.Progress()
                    .Columns(
                    [
                        new SpinnerColumn(),
                        new ElapsedTimeColumn(),
                        new TaskDescriptionColumn()
                    ])
                    .StartAsync(async ctx =>
                    {
                        var task = ctx.AddTask($"[{Constants.EMPHASIS_COLOR}]Running the CI restore script[/]");
                        await RestoreFiles(task, profile);
                    });
            }
        }


        public override async Task PostExecute(ToolProfile? profile, string? action)
        {
            if (Errors.Count == 0)
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]CI {action ?? "process"} complete![/]\n");
            }

            await base.PostExecute(profile, action);
        }


        private async Task StoreFiles(ProgressTask task, ToolProfile? profile)
        {
            if (StopProcessing)
            {
                return;
            }

            string ciScript = scriptBuilder.SetScript(ScriptType.StoreContinuousIntegration).Build();
            await shellRunner.Execute(new(ciScript)
            {
                WorkingDirectory = profile?.WorkingDirectory,
                ErrorHandler = ErrorDataReceived,
                OutputHandler = (o, e) =>
                {
                    if ((e.Data?.Contains("Object type", StringComparison.OrdinalIgnoreCase) ?? false) && e.Data.Any(char.IsDigit))
                    {
                        // Message is something like "Object type 1/84: Module"
                        string[] progressMessage = e.Data.Split(':');
                        if (progressMessage.Length == 0)
                        {
                            return;
                        }

                        string[] progressNumbers = progressMessage[0].Split('/');
                        if (progressNumbers.Length < 2)
                        {
                            return;
                        }

                        double progressCurrent = double.Parse(string.Join("", progressNumbers[0].Where(char.IsDigit)));
                        double progressMax = double.Parse(string.Join("", progressNumbers[1].Where(char.IsDigit)));

                        task.MaxValue = progressMax;
                        task.Value = progressCurrent;
                    }
                }
            }).WaitForExitAsync();
        }


        private async Task RestoreFiles(ProgressTask task, ToolProfile? profile)
        {
            if (StopProcessing)
            {
                return;
            }

            string originalDescription = task.Description;
            string ciScript = scriptBuilder.SetScript(ScriptType.RestoreContinuousIntegration).Build();
            await shellRunner.Execute(new(ciScript)
            {
                WorkingDirectory = profile?.WorkingDirectory,
                ErrorHandler = ErrorDataReceived,
                OutputHandler = (o, e) =>
                {
                    string notFoundString = "The Continuous Integration repository is either not initialized or in an incorrect location on " +
                        "the file system.";
                    if (e.Data?.Contains(notFoundString, StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        // Restore process couldn't find repository directory
                        LogError("The restore process wasn't started because the Continuous Integration repository wasn't found.", o as Process);
                    }
                    else if (e.Data?.Contains("Object type", StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        // Message is something like "Object type Module: updating Activities"
                        task.Description = e.Data;
                    }
                }
            }).WaitForExitAsync();

            task.Description = originalDescription;
        }
    }
}
