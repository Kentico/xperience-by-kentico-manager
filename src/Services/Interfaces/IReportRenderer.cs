namespace Xperience.Manager.Services
{
    /// <summary>
    /// Contains methods for rendering report tables and graphs.
    /// </summary>
    public interface IReportRenderer : IService
    {
        /// <summary>
        /// Displays the enabled administration users table.
        /// </summary>
        Task RenderAdminUserReport(string connectionString);


        /// <summary>
        /// Displays tables related to the CMS_Class database table.
        /// </summary>
        Task RenderClassConsistencyReport(string connectionString);


        /// <summary>
        /// Displays a table containing common Event log errors.
        /// </summary>
        Task RenderEventLogReport(string connectionString);


        /// <summary>
        /// Displays a table showing the largest database tables.
        /// </summary>
        Task RenderTableSizeReport(string connectionString);


        /// <summary>
        /// Displays a table showing the number and count of assets.
        /// </summary>
        Task RenderAssetsReport(string workingDirectory);


        /// <summary>
        /// Displays a bar graph showing channels and their item counts.
        /// </summary>
        Task RenderChannelStatisticsReport(string connectionString);


        /// <summary>
        /// Displays a bar graph showing workspaces and their item counts.
        /// </summary>
        Task RenderWorkspaceReport(string connectionString);
    }
}
