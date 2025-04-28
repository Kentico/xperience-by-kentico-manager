using Spectre.Console;

using Xperience.Manager.Configuration;
using Xperience.Manager.Services;

namespace Xperience.Manager.Commands
{
    public class ReportCommand : AbstractCommand
    {
        private readonly ISqlExecutor sqlExecutor;
        private readonly IAppSettingsManager appSettingsManager;


        public override IEnumerable<string> Keywords => ["r", "report"];


        public override IEnumerable<string> Parameters => [];


        public override string Description => "Displays a report with important project information";


        public override bool RequiresProfile => true;


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal ReportCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public ReportCommand(ISqlExecutor sqlExecutor, IAppSettingsManager appSettingsManager)
        {
            this.sqlExecutor = sqlExecutor;
            this.appSettingsManager = appSettingsManager;
        }


        public override async Task Execute(ToolProfile? profile, string? action) => await RunReport(profile);


        private async Task RunReport(ToolProfile? profile)
        {
            if (StopProcessing)
            {
                return;
            }

            string? connectionString = await appSettingsManager.GetConnectionString(profile, "CMSConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                LogError("Couldn't load connection string.");

                return;
            }

            try
            {
                var largestTables = await sqlExecutor.GetTable(connectionString, "GetLargestTables");
                AnsiConsole.Write(largestTables);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
        }

    }
}
