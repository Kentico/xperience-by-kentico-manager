using NUnit.Framework;
using Xperience.Manager.Configuration;
using Xperience.Manager.Services;

namespace Xperience.Manager.Tests.Services
{
    /// <summary>
    /// Tests for <see cref="IAppSettingsManager"/>.
    /// </summary>
    public class IAppSettingsManagerTests
    {
        private const string DEVELOPMENT_FILE = "appsettings.Development.json";
        private readonly AppSettingsManager appSettingsManager = new();
        private readonly ToolProfile profile = new()
        {
            ProjectName = "Test",
            WorkingDirectory = "./Data"
        };


        [Test]
        public async Task GetConnectionString_GetsStrings()
        {
            string expectedDefaultConnectionString = "initial-connection-string";
            string expectedDevelopmentConnectionString = "initial-dev-connection-string";

            string? defaultConnectionString = await appSettingsManager.GetConnectionString(profile, "CMSConnectionString");
            string? devConnectionString = await appSettingsManager.GetConnectionString(profile, "CMSConnectionString", DEVELOPMENT_FILE);

            Assert.Multiple(() =>
            {
                Assert.That(defaultConnectionString, Is.EqualTo(expectedDefaultConnectionString));
                Assert.That(devConnectionString, Is.EqualTo(expectedDevelopmentConnectionString));
            });
        }


        [Test]
        public async Task GetCmsHeadlessConfiguration_GetsHeadlessKeys()
        {
            var headlessConfig = await appSettingsManager.GetCmsHeadlessConfiguration(profile);

            Assert.Multiple(() =>
            {
                Assert.That(headlessConfig.Enable, Is.True);
                Assert.That(headlessConfig.Caching.SlidingExpiration, Is.EqualTo(60));
            });
        }


        [Test]
        public async Task GetConfigurationKeys_UnsetKey_ActualValueUnset()
        {
            int logKeepDefaultValue = 10;
            var keyToRetrieve = new ConfigurationKey("CMSLogKeepPercent", string.Empty, typeof(string), logKeepDefaultValue);
            var returnedKeys = await appSettingsManager.GetConfigurationKeys(profile, [keyToRetrieve]);
            var logKeepKey = returnedKeys.FirstOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(logKeepKey, Is.Not.Null);
                Assert.That(logKeepKey?.ActualValue, Is.Null);
                Assert.That(logKeepKey?.DefaultValue, Is.EqualTo(logKeepDefaultValue));
            });
        }


        [Test]
        public async Task GetConfigurationKeys_SetKey_ActualValueSet()
        {
            string expectedDefaultHashString = "initial-hash-string";
            string expectedDevelopmentHashString = "initial-dev-hash-string";

            var keyToRetrieve = new ConfigurationKey("CMSHashStringSalt", string.Empty, typeof(string));
            var defaultReturnedKeys = await appSettingsManager.GetConfigurationKeys(profile, [keyToRetrieve]);
            object? defaultHashStringValue = defaultReturnedKeys.FirstOrDefault()?.ActualValue;
            var developmentReturnedKeys = await appSettingsManager.GetConfigurationKeys(profile, [keyToRetrieve], DEVELOPMENT_FILE);
            object? developmentHashStringValue = developmentReturnedKeys.FirstOrDefault()?.ActualValue;

            Assert.Multiple(() =>
            {
                Assert.That(defaultHashStringValue?.ToString(), Is.EqualTo(expectedDefaultHashString));
                Assert.That(developmentHashStringValue?.ToString(), Is.EqualTo(expectedDevelopmentHashString));
            });
        }


        [Test]
        public async Task SetCmsHeadlessConfiguration_UpdatesValue()
        {
            int expectedSlidingExpiration = 100;
            var initialHeadlessConfig = await appSettingsManager.GetCmsHeadlessConfiguration(profile);

            initialHeadlessConfig.Caching.SlidingExpiration = expectedSlidingExpiration;
            await appSettingsManager.SetCmsHeadlessConfiguration(profile, initialHeadlessConfig);
            var updatedHeadlessConfig = await appSettingsManager.GetCmsHeadlessConfiguration(profile);

            Assert.That(updatedHeadlessConfig.Caching.SlidingExpiration, Is.EqualTo(expectedSlidingExpiration));
        }


        [Test]
        public async Task SetConnectionString_SetsConnectionString()
        {
            string connectionStringName = "CMSConnectionString";
            string expectedDefaultConnectionString = "updated-connection-string";
            string expectedDevelopmentConnectionString = "updated-dev-connection-string";

            await appSettingsManager.SetConnectionString(profile, connectionStringName, expectedDefaultConnectionString);
            await appSettingsManager.SetConnectionString(profile, connectionStringName, expectedDevelopmentConnectionString, DEVELOPMENT_FILE);
            string? updatedDefaultConnectionString = await appSettingsManager.GetConnectionString(profile, connectionStringName);
            string? updatedDevelopmentConnectionString = await appSettingsManager.GetConnectionString(profile, connectionStringName, DEVELOPMENT_FILE);

            Assert.Multiple(() =>
            {
                Assert.That(updatedDefaultConnectionString, Is.EqualTo(expectedDefaultConnectionString));
                Assert.That(updatedDevelopmentConnectionString?.ToString(), Is.EqualTo(expectedDevelopmentConnectionString));
            });
        }


        [Test]
        public async Task SetKeyValue_SetsKeyValue()
        {
            string keyName = "CMSAzureAccountName";
            string expectedAzureAccountName = "testAccount";

            await appSettingsManager.SetKeyValue(profile, keyName, expectedAzureAccountName);
            await appSettingsManager.SetKeyValue(profile, keyName, expectedAzureAccountName, DEVELOPMENT_FILE);

            var keyToRetrieve = new ConfigurationKey(keyName, string.Empty, typeof(string));
            var updatedDefaultKeys = await appSettingsManager.GetConfigurationKeys(profile, [keyToRetrieve]);
            object? updatedDefaultAzureAccountName = updatedDefaultKeys.FirstOrDefault()?.ActualValue;
            var updatedDevelopmentKeys = await appSettingsManager.GetConfigurationKeys(profile, [keyToRetrieve], DEVELOPMENT_FILE);
            object? updatedDevelopmentAzureAccountName = updatedDevelopmentKeys.FirstOrDefault()?.ActualValue;

            Assert.Multiple(() =>
            {
                Assert.That(updatedDefaultAzureAccountName?.ToString(), Is.EqualTo(expectedAzureAccountName));
                Assert.That(updatedDevelopmentAzureAccountName?.ToString(), Is.EqualTo(expectedAzureAccountName));
            });
        }
    }
}
