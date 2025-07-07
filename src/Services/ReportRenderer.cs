using Newtonsoft.Json.Linq;

using Spectre.Console;
using Spectre.Console.Rendering;

using Xperience.Manager.Helpers;

namespace Xperience.Manager.Services
{
    public class ReportRenderer : IReportRenderer
    {
        private const int BAR_CHART_WIDTH = 100;
        private const int MAX_COLUMN_CHARS = 200;
        private readonly ISqlExecutor sqlExecutor;


        private static TableBorder BorderStyle => TableBorder.Minimal;


        public ReportRenderer(ISqlExecutor sqlExecutor) =>
            this.sqlExecutor = sqlExecutor;


        public async Task RenderAdminUserReport(string connectionString)
        {
            try
            {
                var enabledAdminUsers = await sqlExecutor.ExecuteQuery(connectionString, "EnabledUsersWithAdminAccess.sql");
                var table = GetTable(enabledAdminUsers);
                RenderSection("Enabled users with admin access", table);
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
                var classResults = new IEnumerable<JObject>[]
                {
                    await sqlExecutor.ExecuteQuery(connectionString, "TablesWithoutClasses.sql"),
                    await sqlExecutor.ExecuteQuery(connectionString, "ClassesWithoutTables.sql")
                };
                string header = "Class consistency";
                if (classResults.All(t => !t.Any()))
                {
                    RenderSection(header, new Markup($"[{Constants.SUCCESS_COLOR}]All classes and tables accounted for![/]"));
                }
                else
                {
                    var tablesToRender = classResults.Where(t => t.Any()).Select(GetTable);
                    RenderSection(header, tablesToRender.ToArray());
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
                var eventLogErrors = await sqlExecutor.ExecuteQuery(connectionString, "CommonEventLogErrors.sql");
                var table = GetTable(eventLogErrors);
                RenderSection("Common Event log errors", table);
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
                var largestTables = await sqlExecutor.ExecuteQuery(connectionString, "GetLargestTables.sql");
                var table = GetTable(largestTables);
                RenderSection("Largest tables", table);
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
                var workspaceStatistics = await sqlExecutor.ExecuteQuery(connectionString, "GetWorkspaceStatistics.sql");
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
                var channelStatistics = await sqlExecutor.ExecuteQuery(connectionString, "GetChannelStatistics.sql");
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


        private Table GetTable(IEnumerable<JObject> objects)
        {
            var table = new Table() { Border = BorderStyle };
            if (!objects.Any())
            {
                return table;
            }

            var firstRow = objects.FirstOrDefault();
            if (firstRow is null)
            {
                return table;
            }

            table.AddColumns(firstRow.Properties().Select(p => $"[{Constants.PROMPT_COLOR}]{p.Name}[/]").ToArray());
            foreach (var row in objects)
            {
                var rowValues = row.Values().Select(GetFormattedValue);
                table
                    .AddEmptyRow() // Add an empty row to simulate padding
                    .AddRow(rowValues.ToArray());
            }

            return table;
        }


        private string GetFormattedValue(JToken token)
        {
            string stringValue = token.Value<string>() ?? string.Empty;
            stringValue = stringValue.Length > MAX_COLUMN_CHARS
                ? stringValue[..MAX_COLUMN_CHARS]
                : stringValue;

            return Markup.Escape(stringValue);
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

            string[] headers = ["Asset type", "Count", "Size (MB)"];
            var table = new Table() { Border = BorderStyle }
                .AddColumns(headers.Select(h => $"[{Constants.PROMPT_COLOR}]{h}[/]").ToArray());
            if (statistics.ContentItemCount > 0)
            {
                table
                    .AddEmptyRow() // Add an empty row to simulate padding
                    .AddRow("Content items", statistics.ContentItemCount.ToString(), statistics.ContentItemSizeMB.ToString("##.##"));
            }

            if (statistics.MediaFileCount > 0)
            {
                table
                    .AddEmptyRow() // Add an empty row to simulate padding
                    .AddRow("Media files", statistics.MediaFileCount.ToString(), statistics.MediaFileSizeMB.ToString("##.##"));
            }

            return table;
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
