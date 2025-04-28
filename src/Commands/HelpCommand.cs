using Spectre.Console;

using System.Reflection;

using Xperience.Manager.Configuration;
using Xperience.Manager.Helpers;

namespace Xperience.Manager.Commands
{
    /// <summary>
    /// A command which displays the current tool version, a list of commands, and their descriptions.
    /// </summary>
    public class HelpCommand : AbstractCommand
    {
        /// <summary>
        /// Workaround for circular dependency when commands are injected into this command.
        /// </summary>
        private readonly IEnumerable<ICommand> commands =
        [
            new ProfileCommand(),
            new InstallCommand(),
            new DeleteCommand(),
            new UpdateCommand(),
            new ContinuousIntegrationCommand(),
            new ContinuousDeploymentCommand(),
            new MacroCommand(),
            new BuildCommand(),
            new SettingsCommand(),
            new CodeGenerateCommand(),
            new ReportCommand()
        ];


        public override IEnumerable<string> Keywords => ["?", "help"];


        public override IEnumerable<string> Parameters => [];


        public override string Description => "Displays the help menu (this screen)";


        public override async Task Execute(ToolProfile? profile, string? action)
        {
            AnsiConsole.Write(
                new FigletText("xman")
                    .LeftJustified()
                    .Color(Color.LightGoldenrod2_1));

            var v = Assembly.GetExecutingAssembly().GetName().Version;
            if (v is not null)
            {
                AnsiConsole.WriteLine($" v{v.Major}.{v.Minor}.{v.Build}");
            }

            AnsiConsole.MarkupInterpolated($" [{Constants.EMPHASIS_COLOR}]https://github.com/Kentico/xperience-by-kentico-manager[/]\n");

            var table = new Table()
                .AddColumn("Command")
                .AddColumn("Parameters")
                .AddColumn("Description");
            foreach (var command in commands)
            {
                table.AddRow(
                    string.Join(", ", command.Keywords),
                    string.Join(", ", command.Parameters),
                    command.Description);
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            if (v is not null)
            {
                var latestVersion = await NuGetVersionHelper.GetLatestVersion("Kentico.Xperience.Manager", v);
                if (latestVersion is not null)
                {
                    AnsiConsole.MarkupInterpolated($" New version [{Constants.SUCCESS_COLOR}]{latestVersion}[/] available!\n\n");
                }
            }
        }
    }
}
