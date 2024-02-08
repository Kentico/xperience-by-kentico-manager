using System.Diagnostics;

using Spectre.Console;

using Xperience.Xman.Configuration;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// Describes an executable command from the CLI.
    /// </summary>
    public abstract class AbstractCommand : ICommand
    {
        public List<string> Errors { get; } = new();


        public bool StopProcessing { get; set; }


        public abstract IEnumerable<string> Keywords { get; }


        public abstract IEnumerable<string> Parameters { get; }


        public abstract string Description { get; }


        public virtual bool RequiresProfile { get; set; }


        public virtual Task PreExecute(ToolProfile? profile, string? action)
        {
            if (RequiresProfile)
            {
                if (profile is null)
                {
                    LogError("This command requires a profile.");
                }
                else
                {
                    PrintCurrentProfile(profile);
                }
            }

            return Task.CompletedTask;
        }


        public abstract Task Execute(ToolProfile? profile, string? action);


        public virtual Task PostExecute(ToolProfile? profile, string? action) => Task.CompletedTask;


        /// <summary>
        /// A handler which can be assigned to <see cref="Process.ErrorDataReceived"/> to handler errors. 
        /// </summary>
        protected void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                LogError(e.Data, sender as Process);
            }
        }


        /// <summary>
        /// Adds an error to <see cref="Errors"/> and stops additional processing.
        /// </summary>
        protected void LogError(string message, Process? process = null)
        {
            Errors.Add(message);

            StopProcessing = true;
            if (process is not null && !process.HasExited)
            {
                process.Kill();
            }
        }


        protected void PrintCurrentProfile(ToolProfile? profile)
        {
            AnsiConsole.Write(new Rule("Current profile:") { Justification = Justify.Left });
            AnsiConsole.MarkupLineInterpolated($"Name: [{Constants.EMPHASIS_COLOR}]{profile?.ProjectName ?? "None"}[/]");
            AnsiConsole.MarkupLineInterpolated($"Path: [{Constants.EMPHASIS_COLOR}]{profile?.WorkingDirectory ?? "None"}[/]");
            AnsiConsole.Write(new Rule());
            AnsiConsole.WriteLine();
        }
    }
}
