using Xperience.Manager.Commands;

namespace Xperience.Manager.Options
{
    /// <summary>
    /// The options used to configure the appsettings.json file, used by <see cref="SettingsCommand"/>.
    /// </summary>
    public class SettingsOptions : IWizardOptions
    {
        public const string CmsHeadlessSetting = "CMSHeadless options";
        public const string ConnectionStringSetting = "Connection string";
        public const string ConfigurationKeysSetting = "Configuration keys";


        /// <summary>
        /// The section of the appsettings.json the user wants to configure.
        /// </summary>
        public string? SettingToChange { get; set; }
    }
}
