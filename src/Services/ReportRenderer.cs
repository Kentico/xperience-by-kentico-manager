using Newtonsoft.Json.Linq;

using Spectre.Console;
using Spectre.Console.Rendering;

using Xperience.Manager.Helpers;

namespace Xperience.Manager.Services
{
    public class ReportRenderer : IReportRenderer
    {
        private const int BAR_CHART_WIDTH = 100;
        private readonly ISqlExecutor sqlExecutor;


        public ReportRenderer(ISqlExecutor sqlExecutor) =>
            this.sqlExecutor = sqlExecutor;


        public async Task RenderAdminUserReport(string connectionString)
        {
            try
            {
                var enabledAdminUsers = await sqlExecutor.GetTable(connectionString, "EnabledUsersWithAdminAccess");
                RenderSection("Enabled users with admin access", enabledAdminUsers);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }


        public async Task RenderClassConsistencyReport(string connectionString)
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }


        public async Task RenderEventLogReport(string connectionString)
        {
            try
            {
                var eventLogErrors = await sqlExecutor.GetTable(connectionString, "CommonEventLogErrors");
                RenderSection("Common Event log errors", eventLogErrors);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }


        public async Task RenderTableSizeReport(string connectionString)
        {
            try
            {
                var largestTables = await sqlExecutor.GetTable(connectionString, "GetLargestTables");
                RenderSection("Largest tables", largestTables);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }


        public async Task RenderWorkspaceReport(string connectionString)
        {
            try
            {
                var workspaceStatistics = await sqlExecutor.ExecuteQuery(connectionString, "GetWorkspaceStatistics");
                var workspaceBarItems = workspaceStatistics.Select(GetWorkspaceStatisticsBarItem);
                RenderSection("Workspace stats (content item count)", new BarChart() { Width = BAR_CHART_WIDTH }
                    .AddItems(workspaceBarItems));
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }


        public async Task RenderChannelStatisticsReport(string connectionString)
        {
            try
            {
                var channelStatistics = await sqlExecutor.ExecuteQuery(connectionString, "GetChannelStatistics");
                var channelBarItems = channelStatistics.Select(GetChannelStatisticsBarItem);
                RenderSection("Channel stats (pages, headless items, emails)", new BarChart() { Width = BAR_CHART_WIDTH }
                    .AddItems(channelBarItems));
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }


        public Task RenderAssetsReport(string workingDirectory)
        {
            try
            {
                var assetStatistics = AssetHelper.GetAssetStatistics(workingDirectory);
                var assetTable = MakeAssetTable(assetStatistics);
                if (assetTable is not null)
                {
                    RenderSection("Assets", assetTable);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return Task.CompletedTask;
        }


        private static BarChartItem GetWorkspaceStatisticsBarItem(JObject row) => new(
            row.Value<string>("WorkspaceDisplayName") ?? string.Empty,
            row.Value<double>("Content items"));


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


        private static void HandleException(Exception ex)
        {
            AnsiConsole.WriteException(ex);
            AnsiConsole.WriteLine();
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
    }
}
