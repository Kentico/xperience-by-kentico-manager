using Xperience.Manager.Configuration;

namespace Xperience.Manager
{
    public static class Constants
    {
        public const int MIN_LISTED_VERSION = 28;

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


        /// <summary>
        /// Configuration keys saved on the root of the appsettings.json file and are all associated with Azure storage.
        /// </summary>
        public static IEnumerable<ConfigurationKey> AzureStorageKeys =>
        [
            new("CMSAzureAccountName",
                "The Azure storage account name",
                typeof(string)),
            new("CMSAzureSharedKey",
                "The Azure storage account shared key",
                typeof(string)),
            new("CMSAzureTempPath",
                "The system uses the specified folder to store temporary files on a local disk, for example when transferring large files " +
                    "to or from the storage account. If not set, the system creates and uses an ~/AzureTemp directory in the project’s root",
                typeof(string)),
            new("CMSAzureCachePath",
                "Specifies a folder on a local disk where files requested from the storage account are cached. This helps minimize the " +
                    "amount of blob storage operations, which saves time and resources. If not set, the system creates and uses an " +
                    "~/AzureCache directory in the project’s root",
                typeof(string)),
            new("CMSAzureBlobEndPoint",
                "Sets the endpoint used for the connection to the blob service of the specified storage account. If you wish to use the " +
                    "default endpoint, remove the setting completely from the appropriate files",
                typeof(string)),
            new("CMSAzurePublicContainer",
                "Indicates if the blob container used to store the application’s files is public. If true, it will be possible to access " +
                    "files directly through the URL of the appropriate blob service, for example: https://<StorageAccountName>.blob.core." +
                    "windows.net/media/imagelibrary/logo.png",
                typeof(bool)),
            new("CMSDownloadBlobTimeout",
                "Specifies the timeout interval in minutes for importing files from Azure Blob storage into Xperience. The default is 1.5 " +
                    "minutes. Increase the interval if you encounter problems when importing large files (2GB+).",
                typeof(int))
        ];


        /// <summary>
        /// Configuration keys saved on the root of the appsettings.json file, but are not associated with common features.
        /// </summary>
        public static IEnumerable<ConfigurationKey> UngroupedKeys =>
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
                "This key determines the extra percentage of events that is retained in the log over the specified limit. This percentage " +
                    "of the oldest events is deleted by batch when the percentage is exceeded",
                typeof(int),
                10),
            new("CMSBuilderScriptsIncludeJQuery",
                "Determines whether the system links the jQuery 3.5.1 library to live site pages containing Page Builder content or forms",
                typeof(bool),
                false),
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
                "https"),
            new("CMSDeleteTemporaryUploadFilesOlderThan",
                "Sets the interval (in hours) at which the system deletes the contents of the temporary folder that stores files uploaded " +
                    "through the Upload file component in Form Builder.",
                typeof(int),
                2),
            new("CMSPhysicalFilesCacheMinutes",
                "Sets client cache expiration time (in minutes) for physical files served by Xperience through the GetResource handler.",
                typeof(int),
                10_080),
            new("CMSStorageProviderAssembly",
                "Configures the assembly name of a custom file system provider.",
                typeof(string))
        ];
    }
}
