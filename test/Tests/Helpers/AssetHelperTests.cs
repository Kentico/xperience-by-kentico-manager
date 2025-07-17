using System.Reflection;

using NUnit.Framework;

using Xperience.Manager.Helpers;

namespace Xperience.Manager.Tests.Helpers
{
    /// <summary>
    /// Tests for <see cref="AssetHelper"/>.
    /// </summary>
    public class AssetHelperTests
    {
        [Test]
        public void GetAssetStatistics_GetsAssets()
        {
            var statistics = AssetHelper.GetAssetStatistics(GetWorkingDirectory());
            var mediaStatistic = statistics.FirstOrDefault(s => s.DirectoryName?.Equals("media") ?? false);
            var contentItemStatistic = statistics.FirstOrDefault(s => s.DirectoryName?.Equals("contentitems") ?? false);

            Assert.Multiple(() =>
            {
                Assert.That(statistics.Count(), Is.EqualTo(2));
                Assert.That(mediaStatistic?.Exists, Is.True);
                Assert.That(mediaStatistic?.FileCount, Is.EqualTo(3));
                Assert.That(mediaStatistic?.DirectorySizeMB, Is.InRange(0.0029, 0.003));
                Assert.That(contentItemStatistic?.Exists, Is.True);
                Assert.That(contentItemStatistic?.FileCount, Is.EqualTo(2));
                Assert.That(contentItemStatistic?.DirectorySizeMB, Is.InRange(0.0019, 0.002));
            });
        }


        [Test]
        public void GetAssetStatistics_CustomDir_NoMedia_GetsAssetsWithoutMedia()
        {
            var statistics = AssetHelper.GetAssetStatistics(GetWorkingDirectory(), "customAssetDir");
            var mediaStatistic = statistics.FirstOrDefault(s => s.DirectoryName?.Equals("media") ?? false);
            var contentItemStatistic = statistics.FirstOrDefault(s => s.DirectoryName?.Equals("contentitems") ?? false);

            Assert.Multiple(() =>
            {
                Assert.That(statistics.Count(), Is.EqualTo(2));
                Assert.That(mediaStatistic?.Exists, Is.False);
                Assert.That(contentItemStatistic?.Exists, Is.True);
                Assert.That(contentItemStatistic?.FileCount, Is.EqualTo(1));
                Assert.That(contentItemStatistic?.DirectorySizeMB, Is.InRange(0.0009, 0.001));
            });
        }


        private static string GetWorkingDirectory()
        {
            string? location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(location))
            {
                throw new InvalidOperationException("Execution path not found.");
            }

            return Path.Combine(location, "Data");
        }
    }
}
