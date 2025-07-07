namespace Xperience.Manager.Helpers
{
    public static class AssetHelper
    {
        private const string ASSET_DIRNAME = "assets";
        private const string MEDIA_DIRNAME = "media";
        private const string CONTENT_ITEM_DIRNAME = "contentitems";


        /// <summary>
        /// Gets the statistics for physical files in the provided assets folder.
        /// </summary>
        /// <param name="workingDirectory">The root directory of the Xperience by Kentico instance.</param>
        /// <param name="customDirectoryName">The name of the folder containing assets, if not using the default name.</param>
        public static IEnumerable<AssetStatistic> GetAssetStatistics(string workingDirectory, string? customDirectoryName)
        {
            customDirectoryName ??= ASSET_DIRNAME;
            string assetDir = Path.Combine(workingDirectory, customDirectoryName);
            if (!Directory.Exists(assetDir))
            {
                return [];
            }

            var result = new List<AssetStatistic>();
            AddStatistic(result, assetDir, MEDIA_DIRNAME);
            AddStatistic(result, assetDir, CONTENT_ITEM_DIRNAME);

            return result;
        }


        /// <summary>
        /// Returns <c>true</c> if the <see cref="ASSET_DIRNAME"/> folder exists in the project.
        /// </summary>
        /// <param name="workingDirectory">The root directory of the Xperience by Kentico instance.</param>
        public static bool DefaultDirectoryExists(string workingDirectory)
        {
            string assetDir = Path.Combine(workingDirectory, ASSET_DIRNAME);

            return Directory.Exists(assetDir);
        }


        private static void AddStatistic(List<AssetStatistic> statistics, string assetsPath, string directoryName)
        {
            string fullPath = Path.Combine(assetsPath, directoryName);
            bool exists = Directory.Exists(fullPath);
            var statistic = new AssetStatistic()
            {
                Exists = exists,
                DirectoryName = directoryName
            };

            if (exists)
            {
                var dirInfo = new DirectoryInfo(fullPath);
                var allFiles = dirInfo.GetFiles("*", SearchOption.AllDirectories);
                statistic.FileCount = allFiles.Length;
                statistic.DirectorySizeMB = allFiles.Sum(f => f.Length) / (double)1048576;
            }

            statistics.Add(statistic);
        }
    }


    /// <summary>
    /// Represents the count and size of Xperience by Kentico asset folders.
    /// </summary>
    public class AssetStatistic
    {
        /// <summary>
        /// If <c>false</c>, the folder is not present on the filesystem.
        /// </summary>
        public bool Exists { get; set; }


        /// <summary>
        /// The name of the directory.
        /// </summary>
        public string? DirectoryName { get; set; }


        /// <summary>
        /// The number of files in the folder.
        /// </summary>
        public int FileCount { get; set; }


        /// <summary>
        /// The total size of the files in the folder, in megabytes.
        /// </summary>
        public double DirectorySizeMB { get; set; }
    }
}
