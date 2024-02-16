using Xperience.Manager.Services;

namespace Xperience.Manager.Options
{
    /// <summary>
    /// The options used to run a SQL query via the <see cref="ScriptBuilder.RUN_SQL_QUERY"/> script. 
    /// </summary>
    public class RunSqlOptions : IWizardOptions
    {
        /// <summary>
        /// The connection string which determines the database to connect to.
        /// </summary>
        public string? ConnString { get; set; }


        /// <summary>
        /// The query to execute.
        /// </summary>
        public string? SqlQuery { get; set; }
    }
}
