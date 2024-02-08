using Spectre.Console;

using Xperience.Xman.Configuration;
using Xperience.Xman.Options;
using Xperience.Xman.Services;
using Xperience.Xman.Wizards;

namespace Xperience.Xman.Commands
{
    public class ProfileCommand : AbstractCommand
    {
        private const string ADD = "add";
        private const string DELETE = "delete";
        private const string SWITCH = "switch";
        private readonly IConfigManager configManager;
        private readonly IWizard<NewProfileOptions> wizard;


        public override IEnumerable<string> Keywords => ["p", "profile"];


        public override IEnumerable<string> Parameters => [ADD, DELETE, SWITCH];


        public override string Description => "Manage and switch installation profiles";


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal ProfileCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public ProfileCommand(IConfigManager configManager, IWizard<NewProfileOptions> wizard)
        {
            this.configManager = configManager;
            this.wizard = wizard;
        }


        public override async Task PreExecute(ToolProfile? profile, string? action)
        {
            if (StopProcessing)
            {
                return;
            }

            action ??= SWITCH;
            if (!Parameters.Any(p => p.Equals(action, StringComparison.OrdinalIgnoreCase)))
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

            action ??= SWITCH;
            var config = await configManager.GetConfig();

            // Can't switch or delete if there are no profiles
            if (!config.Profiles.Any() &&
                (action.Equals(SWITCH, StringComparison.OrdinalIgnoreCase) || action.Equals(DELETE, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLineInterpolated($"There are no registered profiles. Install a new instance with [{Constants.SUCCESS_COLOR}]xman i[/] to add a profile.\n");
                return;
            }

            if (config.Profiles.Count == 1 && action.Equals(SWITCH, StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.WriteLine("You're currently using the only registered profile.\n");
                return;
            }

            if (action?.Equals(SWITCH, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                await SwitchProfile(config.Profiles, profile);
            }
            else if (action?.Equals(ADD, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                await AddProfile();
            }
            else if (action?.Equals(DELETE, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                await DeleteProfile(config.Profiles);
            }
        }


        public override async Task PostExecute(ToolProfile? profile, string? action)
        {
            AnsiConsole.WriteLine();

            await base.PostExecute(profile, action);
        }


        private async Task AddProfile()
        {
            if (StopProcessing)
            {
                return;
            }

            var options = await wizard.Run();
            if (!Directory.Exists(options.WorkingDirectory))
            {
                LogError($"The directory {options.WorkingDirectory} couldn't be found.");
                return;
            }

            await configManager.AddProfile(new()
            {
                ProjectName = options.Name,
                WorkingDirectory = options.WorkingDirectory
            });

            AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Profile '{options.Name}' added[/]");
        }


        private async Task DeleteProfile(List<ToolProfile> profiles)
        {
            if (StopProcessing)
            {
                return;
            }

            var profile = AnsiConsole.Prompt(new SelectionPrompt<ToolProfile>()
                .Title("Delete which [green]profile[/]?")
                .PageSize(10)
                .UseConverter(p => p.ProjectName ?? string.Empty)
                .MoreChoicesText("Scroll for more...")
                .AddChoices(profiles));

            await configManager.RemoveProfile(profile);

            AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Profile '{profile.ProjectName}' deleted[/]");
        }


        private async Task SwitchProfile(List<ToolProfile> profiles, ToolProfile? profile)
        {
            if (StopProcessing)
            {
                return;
            }

            PrintCurrentProfile(profile);

            var prompt = new SelectionPrompt<ToolProfile>()
                .Title("Switch to profile:")
                .PageSize(10)
                .UseConverter(p => p.ProjectName ?? string.Empty)
                .MoreChoicesText("Scroll for more...")
                .AddChoices(profiles);

            var newProfile = AnsiConsole.Prompt(prompt);
            await configManager.SetCurrentProfile(newProfile);

            AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Switched to '{newProfile.ProjectName}'[/]");
        }
    }
}
