using Xperience.Manager.Configuration;

namespace Xperience.Manager
{
    public static class Constants
    {
        public const string CONFIG_FILENAME = "xman.json";

        public const string CD_FILES_DIR = "CDRepository";
        public const string CD_CONFIG_NAME = "repository.config";
        public const string CD_CONFIG_DIR = "ContinuousDeployment";

        public const string SUCCESS_COLOR = "green";
        public const string ERROR_COLOR = "red";
        public const string EMPHASIS_COLOR = "deepskyblue3_1";
        public const string PROMPT_COLOR = "lightgoldenrod2_2";

        public const string DATABASE_TOOL = "Kentico.Xperience.DbManager";
        public const string TEMPLATES_PACKAGE = "kentico.xperience.templates";
        public const string TEMPLATE_SAMPLE = "kentico-xperience-sample-mvc";
        public const string TEMPLATE_BLANK = "kentico-xperience-mvc";
        public const string TEMPLATE_ADMIN = "kentico-xperience-admin-sample";

        public static IEnumerable<ConfigurationKey> ConfigurationKeys =>
        [
            new("CMSForbiddenURLValues",
                "Specifies characters that are forbidden in page URLs",
                typeof(string),
                "\\/:*?\"<>|&%.'#[]+ \t=„“"),
            new("CMSHashStringSalt",
                "Sets the salt value the system uses in hash functions, for example when creating macro signatures",
                typeof(string)),
            new("CMSImageExtensions",
                "Specifies the file extensions that the system recognizes as image files",
                typeof(string),
                "bmp;gif;ico;png;wmf;jpg;jpeg;tiff;tif;webp"),
            new("CMSLogKeepPercent",
                "This key determines the extra percentage of events that is retained in the log over the specified limit. This percentage of the oldest events is deleted by batch when the percentage is exceeded",
                typeof(int),
                10),
            new("CMSCIEncoding",
                "Sets the character encoding used when the CI/CD features generate non-binary files in the repository folder",
                typeof(string),
                "utf-8"),
            new("CMSCIRepositoryPath",
                "Sets the location of the Continuous Integration file repository root folder",
                typeof(string),
                "App_Data\\CIRepository"),
            new("CMSProcessContactActionsInterval",
                "Sets the interval (in seconds) in which contact data and activities are batch processed by the system",
                typeof(int),
                10),
            new("CMSCreateContactActionsLogWorker",
                "If enabled, all web farm servers recalculate contact scores, contact groups, personas, and marketing automation triggers",
                typeof(bool),
                true),
            new("CMSEmailUrlDefaultScheme",
                "Sets the scheme (protocol) used when resolving relative URLs within email content",
                typeof(string),
                "https")
        ];
    }
}
