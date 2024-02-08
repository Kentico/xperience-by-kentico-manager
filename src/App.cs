using Newtonsoft.Json;

using Spectre.Console;

using Xperience.Xman.Repositories;
using Xperience.Xman.Services;

namespace Xperience.Xman
{
    /// <summary>
    /// The main entry point for the console application which supports Dependency Injection.
    /// </summary>
    public class App
    {
        private readonly IConfigManager configManager;
        private readonly ICommandRepository commandRepository;


        public App(IConfigManager configManager, ICommandRepository commandRepository)
        {
            this.configManager = configManager;
            this.commandRepository = commandRepository;
        }


        /// <summary>
        /// Runs the console application with the arguments provided by the user.
        /// </summary>
        public async Task Run(string[] args)
        {
            AnsiConsole.WriteLine();
            try
            {
                await configManager.EnsureConfigFile();
            }
            catch (JsonReaderException)
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]There was an error reading the tool config file {Constants.CONFIG_FILENAME}. Please delete or rename the file and migrate your configuration into the new file created on first run.[/]\n");
                return;
            }

            string identifier = "help";
            if (args.Length > 0)
            {
                identifier = args[0];
            }

            var command = commandRepository.Get(identifier);
            if (command is null)
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]Invalid command '{identifier}'[/]\n");
                return;
            }

            try
            {
                var activeProfile = await configManager.GetCurrentProfile();
                string? actionName = args.Length > 1 ? args[1].ToLower() : null;
                await command.PreExecute(activeProfile, actionName);
                if (!command.StopProcessing)
                {
                    await command.Execute(activeProfile, actionName);
                }

                if (!command.StopProcessing)
                {
                    await command.PostExecute(activeProfile, actionName);
                }

                if (command.Errors.Any())
                {
                    AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]Process failed with errors:\n{string.Join("\n", command.Errors)}[/]\n");
                }
            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]Process failed with error:\n{e.Message}[/]\n");
            }
        }
    }
}
