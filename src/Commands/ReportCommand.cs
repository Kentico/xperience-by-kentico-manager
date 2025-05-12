using Newtonsoft.Json.Linq;

using Spectre.Console;
using Spectre.Console.Rendering;

using Xperience.Manager.Configuration;
using Xperience.Manager.Helpers;
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

            string? workingDirectory = profile?.WorkingDirectory;
            if (string.IsNullOrEmpty(workingDirectory))
            {
                LogError("Couldn't load working directory.");

                return;
            }

            await AnsiConsole.Status().StartAsync("Running reports...", (ctx) => RunReportInternal(ctx, connectionString, workingDirectory));
        }


        private async Task RunReportInternal(StatusContext ctx, string connectionString, string workingDirectory)
        {
            try
            {
                ctx.Status = "Checking class consistency...";
                var classResults = new Table[]
                {
                    await sqlExecutor.GetTable(connectionString, "TablesWithoutClasses"),
                    await sqlExecutor.GetTable(connectionString, "ClassesWithoutTables")
                };
                string header = "Class consistency";
                if (classResults.All(t => t.Rows.Count == 0))
                {
                    RenderSection(header, new Markup($"[{Constants.SUCCESS_COLOR}]All classes and tables accounted for![/]"));
                }
                else
                {
                    RenderSection(header, classResults.Where(t => t.Rows.Count > 0).ToArray());
                }

                ctx.Status = "Checking assets...";
                var assetStatistics = AssetHelper.GetAssetStatistics(workingDirectory);
                var assetTable = MakeAssetTable(assetStatistics);
                if (assetTable is not null)
                {
                    RenderSection("Assets", assetTable);
                }

                ctx.Status = "Getting channel statistics...";
                var channelStatistics = await sqlExecutor.ExecuteQuery(connectionString, "GetChannelStatistics");
                var channelBarItems = channelStatistics.Select(GetChannelStatisticsBarItem);
                RenderSection("Channel stats (pages, headless items, emails)", new BarChart() { Width = 100 }.AddItems(channelBarItems));

                ctx.Status = "Getting Workspace statistics...";
                var workspaceStatistics = await sqlExecutor.ExecuteQuery(connectionString, "GetWorkspaceStatistics");
                var workspaceBarItems = workspaceStatistics.Select(GetWorkspaceStatisticsBarItem);
                RenderSection("Workspace stats (content item count)", new BarChart() { Width = 100 }.AddItems(workspaceBarItems));

                ctx.Status = "Getting admin users...";
                var enabledAdminUsers = await sqlExecutor.GetTable(connectionString, "EnabledUsersWithAdminAccess");
                RenderSection("Enabled users with admin access", enabledAdminUsers);

                ctx.Status = "Getting large tables...";
                var largestTables = await sqlExecutor.GetTable(connectionString, "GetLargestTables");
                RenderSection("Largest tables", largestTables);

                ctx.Status = "Getting Event log errors...";
                var eventLogErrors = await sqlExecutor.GetTable(connectionString, "CommonEventLogErrors");
                RenderSection("Common Event log errors", eventLogErrors);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                AnsiConsole.WriteLine();
            }
        }


        private static Table? MakeAssetTable(AssetStatistics? statistics)
        {
            if (statistics is null)
            {
                return null;
            }

            if (statistics.ContentItemCount == 0 && statistics.MediaFileCount == 0)
            {
                return null;
            }

            var result = new Table().AddColumns("Asset type", "Count", "Size (MB)");
            if (statistics.ContentItemCount > 0)
            {
                result.AddRow("Content items", statistics.ContentItemCount.ToString(), statistics.ContentItemSizeMB.ToString("##.##"));
            }

            if (statistics.MediaFileCount > 0)
            {
                result.AddRow("Media files", statistics.MediaFileCount.ToString(), statistics.MediaFileSizeMB.ToString("##.##"));
            }

            return result;
        }


        private static void RenderSection(string header, params IRenderable[] renderables)
        {
            AnsiConsole.Write(new Rule(header)
            {
                Justification = Justify.Left,
                Style = Style.Parse(Constants.EMPHASIS_COLOR)
            });
            foreach (var renderable in renderables)
            {
                var padder = new Padder(renderable, new Padding(5, 1, 0, 1));
                AnsiConsole.Write(padder);
            }
        }


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
