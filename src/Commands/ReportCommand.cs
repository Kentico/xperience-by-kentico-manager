using Newtonsoft.Json.Linq;

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

            await AnsiConsole.Status().StartAsync("Running reports...", (ctx) => RunReportInternal(ctx, connectionString));
        }


        private async Task RunReportInternal(StatusContext ctx, string connectionString)
        {
            try
            {
                ctx.Status = "Getting channel statistics...";
                var channelStatistics = await sqlExecutor.ExecuteQuery(connectionString, "GetChannelStatistics");
                var channelBarItems = channelStatistics.Select(GetChannelStatisticsBarItem);
                WriteTableHeader("Channel stats (pages, headless items, emails)");
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new BarChart() { Width = 100 }.AddItems(channelBarItems));
                AnsiConsole.WriteLine();

                ctx.Status = "Getting Workspace statistics...";
                var workspaceStatistics = await sqlExecutor.ExecuteQuery(connectionString, "GetWorkspaceStatistics");
                var workspaceBarItems = workspaceStatistics.Select(GetWorkspaceStatisticsBarItem);
                WriteTableHeader("Workspace stats (content item count)");
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new BarChart() { Width = 100 }.AddItems(workspaceBarItems));
                AnsiConsole.WriteLine();

                ctx.Status = "Getting admin users...";
                var enabledAdminUsers = await sqlExecutor.GetTable(connectionString, "EnabledUsersWithAdminAccess");
                WriteTableHeader("Enabled users with admin access");
                AnsiConsole.Write(enabledAdminUsers);
                AnsiConsole.WriteLine();

                ctx.Status = "Getting large tables...";
                var largestTables = await sqlExecutor.GetTable(connectionString, "GetLargestTables");
                WriteTableHeader("Largest tables");
                AnsiConsole.Write(largestTables);
                AnsiConsole.WriteLine();

                ctx.Status = "Getting recent errors...";
                var eventLogErrors = await sqlExecutor.GetTable(connectionString, "RecentEventLogErrors");
                WriteTableHeader("Latest Event Log errors");
                AnsiConsole.Write(eventLogErrors);
                AnsiConsole.WriteLine();
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                AnsiConsole.WriteLine();
            }
        }


        private static void WriteTableHeader(string header) =>
            AnsiConsole.Write(new Rule(header)
            {
                Justification = Justify.Left,
                Style = Style.Parse(Constants.EMPHASIS_COLOR)
            });


        private static BarChartItem GetChannelStatisticsBarItem(JObject row)
        {
            string channelType = row.Value<string>("ChannelType") ?? "Website";
            var color = channelType switch
            {
                "Website" => Color.Red,
                "Email" => Color.Green,
                "Headless" => Color.Yellow,
                _ => Color.Red
            };

            string label = $"{row.Value<string>("ChannelName") ?? string.Empty} ({channelType})";

            return new(
                label,
                row.Value<double>("Statistic"),
                color);
        }


        private static BarChartItem GetWorkspaceStatisticsBarItem(JObject row) => new(
            row.Value<string>("WorkspaceDisplayName") ?? string.Empty,
            row.Value<double>("Content items"));
    }
}
