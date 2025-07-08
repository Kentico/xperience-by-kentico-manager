using NSubstitute;

using NUnit.Framework;

using Xperience.Manager.Services;

namespace Xperience.Manager.Tests.Services
{
    /// <summary>
    /// Tests of <see cref="IReportRenderer"/>.
    /// </summary>
    public class IReportRendererTests
    {
        private ReportRenderer reportRenderer;
        private readonly ISqlExecutor sqlExecutor = Substitute.For<ISqlExecutor>();
        private const string CONNSTRING = "FAKE_CONN_STRING";


        [OneTimeSetUp]
        public void OneTimeSetUp() =>
            reportRenderer = new ReportRenderer(sqlExecutor);


        [Test]
        public async Task RenderAdminUserReport_RunsQuery()
        {
            await reportRenderer.RenderAdminUserReport(CONNSTRING);

            await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "EnabledUsersWithAdminAccess.sql");
        }


        [Test]
        public async Task RenderClassConsistencyReport_RunsQuery()
        {
            await reportRenderer.RenderClassConsistencyReport(CONNSTRING);

            await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "TablesWithoutClasses.sql");
            await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "ClassesWithoutTables.sql");
        }


        [Test]
        public async Task RenderEventLogReport_RunsQuery()
        {
            await reportRenderer.RenderEventLogReport(CONNSTRING);

            await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "CommonEventLogErrors.sql");
        }


        [Test]
        public async Task RenderTableSizeReport_RunsQuery()
        {
            await reportRenderer.RenderTableSizeReport(CONNSTRING);

            await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "GetLargestTables.sql");
        }


        [Test]
        public async Task RenderWorkspaceReport_RunsQuery()
        {
            await reportRenderer.RenderWorkspaceReport(CONNSTRING);

            await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "GetWorkspaceStatistics.sql");
        }


        [Test]
        public async Task RenderChannelStatisticsReport_RunsQuery()
        {
            await reportRenderer.RenderChannelStatisticsReport(CONNSTRING);

            await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "GetChannelStatistics.sql");
        }
    }
}
