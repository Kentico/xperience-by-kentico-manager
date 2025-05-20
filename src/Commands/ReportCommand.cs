using Spectre.Console;

using Xperience.Manager.Configuration;
using Xperience.Manager.Services;

namespace Xperience.Manager.Commands
{
    public class ReportCommand : AbstractCommand
    {
        private readonly IReportRenderer reportRenderer;
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


        public ReportCommand(IReportRenderer reportRenderer, IAppSettingsManager appSettingsManager)
        {
            this.reportRenderer = reportRenderer;
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
            ctx.Status = "Checking class consistency...";
            await reportRenderer.RenderClassConsistencyReport(connectionString);

            ctx.Status = "Checking assets...";
            await reportRenderer.RenderAssetsReport(workingDirectory);

            ctx.Status = "Getting channel statistics...";
            await reportRenderer.RenderChannelStatisticsReport(connectionString);

            ctx.Status = "Getting Workspace statistics...";
            await reportRenderer.RenderWorkspaceReport(connectionString);

            ctx.Status = "Getting admin users...";
            await reportRenderer.RenderAdminUserReport(connectionString);

            ctx.Status = "Getting large tables...";
            await reportRenderer.RenderTableSizeReport(connectionString);

            ctx.Status = "Getting Event log errors...";
            await reportRenderer.RenderEventLogReport(connectionString);
        }
    }
}
