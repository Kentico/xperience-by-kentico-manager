using Xperience.Manager.Commands;

namespace Xperience.Manager.Options
{
    /// <summary>
    /// The options used to update Xperience by Kentico project files and databases, used by <see cref="UpdateCommand"/>.
    /// </summary>
    public class UpdateOptions : IWizardOptions
    {
        /// <summary>
        /// The version of the Xperience by Kentico packages and database to update to.
        /// </summary>
        public Version? Version { get; set; }


        /// <summary>
        /// The name of the package to update.
        /// </summary>
        public string? PackageName { get; set; }
    }
}