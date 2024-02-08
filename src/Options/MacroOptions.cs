using Xperience.Xman.Commands;

namespace Xperience.Xman.Options
{
    /// <summary>
    /// The options used to re-sign macros in <see cref="MacroCommand"/>. 
    /// </summary>
    public class MacroOptions : IWizardOptions
    {
        /// <summary>
        /// If <c>true</c>, all macros are signed with the <see cref="UserName"/>.
        /// </summary>
        public bool SignAll { get; set; }


        /// <summary>
        /// The user to sign macros when <see cref="SignAll"/> is <c>true</c>.
        /// </summary>
        public string? UserName { get; set; }


        /// <summary>
        /// The salt that was used to sign existing macros when <see cref="SignAll"/> is <c>false</c>.
        /// </summary>
        public string? OldSalt { get; set; }


        /// <summary>
        /// The optional new salt to use. If not provided, the salt from appsettings is used.
        /// </summary>
        public string? NewSalt { get; set; }
    }
}
