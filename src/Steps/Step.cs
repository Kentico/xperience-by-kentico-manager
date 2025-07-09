using Spectre.Console;

namespace Xperience.Manager.Steps
{
    /// <summary>
    /// A step used to display a prompt for user interaction and optionally return the value.
    /// </summary>
    public class Step<T>(StepOptions<T> options) : IStep
    {
        public Task Execute()
        {
            if (options.Prompt is null)
            {
                return Task.CompletedTask;
            }

            if (options.SkipChecker is not null && options.SkipChecker())
            {
                return Task.CompletedTask;
            }

            var input = AnsiConsole.Prompt(options.Prompt);
            if (options.ValueReceiver is not null)
            {
                options.ValueReceiver(input);
            }

            return Task.CompletedTask;
        }
    }
}
