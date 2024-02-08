using Xperience.Xman.Commands;

namespace Xperience.Xman.Options
{
    /// <summary>
    /// The options used to install Xperience by Kentico project files and databases, used by <see cref="InstallCommand"/>.
    /// </summary>
    public class InstallOptions : IWizardOptions
    {
        /// <summary>
        /// The version of the Xperience by Kentico templates and database to install.
        /// </summary>
        public Version? Version { get; set; }


        /// <summary>
        /// The name of the template to install.
        /// </summary>
        public string Template { get; set; } = "kentico-xperience-sample-mvc";


        /// <summary>
        /// The name of the Xperience by Kentico project.
        /// </summary>
        public string ProjectName { get; set; } = "xbk";


        /// <summary>
        /// The absolute path of the parent directory to install the project within. E.g. if set to
        /// "C:\inetpub\wwwroot" a new installation will be created in "C:\inetpub\wwwroot\projectname."
        /// </summary>
        public string InstallRootPath { get; set; } = Environment.CurrentDirectory;


        /// <summary>
        /// If <c>true</c>, the "--cloud" parameter is used during installation.
        /// </summary>
        public bool UseCloud { get; set; } = false;


        /// <summary>
        /// The name of the new database.
        /// </summary>
        public string DatabaseName { get; set; } = "xperience";


        /// <summary>
        /// The name of the SQL server to use.
        /// </summary>
        public string? ServerName { get; set; }


        /// <summary>
        /// The name of the global administrator password.
        /// </summary>
        public string AdminPassword { get; set; } = "test";
    }
}
