using Xperience.Xman.Commands;

namespace Xperience.Xman.Options
{
    /// <summary>
    /// The options used to create a new profile in <see cref="ProfileCommand"/>. 
    /// </summary>
    public class NewProfileOptions : IWizardOptions
    {
        /// <summary>
        /// The name of the new profile.
        /// </summary>
        public string? Name { get; set; }


        /// <summary>
        /// The absolute path containing the Xperience by Kentico project.
        /// </summary>
        public string? WorkingDirectory { get; set; }
    }
}
