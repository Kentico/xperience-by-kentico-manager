using Xperience.Manager.Commands;

namespace Xperience.Manager.Options
{
    /// <summary>
    /// The options used to configure the application settings file, used by <see cref="SettingsCommand"/>.
    /// </summary>
    public class SettingsOptions : IWizardOptions
    {
        public const string CmsHeadlessSetting = "CMSHeadless options";
        public const string ConnectionStringSetting = "Connection string";
        public const string UngroupedKeySetting = "Configuration keys";
        public const string AzureStorageSetting = "Azure storage";


        /// <summary>
        /// The section of the application settings the user wants to configure.
        /// </summary>
        public string? SettingToChange { get; set; }


        /// <summary>
        /// The name of the file to modify.
        /// </summary>
        public string? AppSettingsFileName { get; set; }
    }
}
