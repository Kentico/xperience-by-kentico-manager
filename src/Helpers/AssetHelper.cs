namespace Xperience.Manager.Helpers
{
    public static class AssetHelper
    {
        private const string ASSET_DIRNAME = "assets";
        private const string MEDIA_DIRNAME = "media";
        private const string CONTENT_ITEM_DIRNAME = "contentitems";


        /// <summary>
        /// Gets the statistics for physical files in the <see cref="ASSET_DIRNAME"/> folder.
        /// </summary>
        /// <param name="workingDirectory">The root directory of the Xperience by Kentico instance.</param>
        public static AssetStatistics? GetAssetStatistics(string workingDirectory)
        {
            string assetDir = Path.Combine(workingDirectory, ASSET_DIRNAME);
            if (!Directory.Exists(assetDir))
            {
                return null;
            }

            var result = new AssetStatistics();
            string contentItemAssetDir = Path.Combine(assetDir, CONTENT_ITEM_DIRNAME);
            if (Directory.Exists(contentItemAssetDir))
            {
                var assetDirInfo = new DirectoryInfo(contentItemAssetDir);
                var contentItemAssets = assetDirInfo.EnumerateFiles("*", SearchOption.AllDirectories);
                result.ContentItemCount = contentItemAssets.Count();
                result.ContentItemSizeMB = contentItemAssets.Sum(f => f.Length) / (double)1048576;
            }

            string mediaFileDir = Path.Combine(assetDir, MEDIA_DIRNAME);
            if (Directory.Exists(mediaFileDir))
            {
                var mediaDirInfo = new DirectoryInfo(mediaFileDir);
                var mediaFiles = mediaDirInfo.EnumerateFiles("*", SearchOption.AllDirectories);
                result.MediaFileCount = mediaFiles.Count();
                result.MediaFileSizeMB = mediaFiles.Sum(f => f.Length) / (double)1048576;
            }

            return result;
        }
    }


    /// <summary>
    /// Represents the count and size of Xperience by Kentico asset folders.
    /// </summary>
    public class AssetStatistics
    {
        /// <summary>
        /// The number of files in the content item asset folder.
        /// </summary>
        public int ContentItemCount { get; set; }


        /// <summary>
        /// The number of files in the media files folder.
        /// </summary>
        public int MediaFileCount { get; set; }


        /// <summary>
        /// The total size of the files in the content item asset folder, in megabytes.
        /// </summary>
        public double ContentItemSizeMB { get; set; }


        /// <summary>
        /// The total size of the files in the media files folder, in megabytes.
        /// </summary>
        public double MediaFileSizeMB { get; set; }
    }
}
