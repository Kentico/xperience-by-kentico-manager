using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Xperience.Manager.Helpers
{
    public static class NuGetVersionHelper
    {
        private const string NUGET_SOURCE = "https://api.nuget.org/v3/index.json";


        /// <summary>
        /// Gets all versions of the provided NuGet package.
        /// </summary>
        public static async Task<IEnumerable<NuGetVersion>> GetPackageVersions(string package)
        {
            var repository = Repository.Factory.GetCoreV3(NUGET_SOURCE);
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

            return await resource.GetAllVersionsAsync(
                package,
                new SourceCacheContext(),
                NullLogger.Instance,
                CancellationToken.None);
        }


        /// <summary>
        /// Gets the latest version of the provided NuGet package, or <n>null</n> if <paramref name="currentVersion"/>
        /// is the latest version.
        /// </summary>
        public static async Task<Version?> GetLatestVersion(string package, Version currentVersion)
        {
            var repository = Repository.Factory.GetCoreV3(NUGET_SOURCE);
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
            var versions = await resource.GetAllVersionsAsync(
                package,
                new SourceCacheContext(),
                NullLogger.Instance,
                CancellationToken.None);

            var latest = versions.OrderByDescending(v => v).FirstOrDefault()?.Version ?? currentVersion;

            return latest > currentVersion ? latest : null;
        }
    }
}
