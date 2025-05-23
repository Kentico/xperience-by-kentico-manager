﻿using NUnit.Framework;

using Xperience.Manager.Configuration;
using Xperience.Manager.Services;

namespace Xperience.Manager.Tests.Services
{
    /// <summary>
    /// Tests for <see cref="IConfigManager"/>.
    /// </summary>
    public class IConfigManagerTests : TestBase
    {
        private readonly ConfigManager configManager = new();


        [Test]
        public async Task EnsureConfigFile_CreatesFile()
        {
            await configManager.EnsureConfigFile();

            Assert.That(File.Exists(Constants.CONFIG_FILENAME));
        }


        [Test]
        public async Task GetCurrentProfile_SingleProfile_GetsProfileAndUpdatesFile()
        {
            File.Copy("Data/config_with_one_profile.json", Constants.CONFIG_FILENAME);

            var profile = await configManager.GetCurrentProfile();
            var config = await configManager.GetConfig();

            Assert.That(profile, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(profile.ProjectName, Is.EqualTo("1"));
                Assert.That(config.CurrentProfile, Is.EqualTo("1"));
            });
        }

        [Test]
        public async Task AddProfile_AddsProfiles()
        {
            await configManager.EnsureConfigFile();
            ToolProfile p1 = new()
            {
                ProjectName = "1",
                WorkingDirectory = "C:\\1"
            };
            await configManager.AddProfile(p1);

            ToolProfile p2 = new()
            {
                ProjectName = "2",
                WorkingDirectory = "C:\\2"
            };
            await configManager.AddProfile(p2);
            var config = await configManager.GetConfig();

            Assert.That(config.Profiles, Has.Count.EqualTo(2));
        }


        [Test]
        public void AddProfile_ExistingName_ThrowsException()
        {
            File.Copy("Data/config_with_one_profile.json", Constants.CONFIG_FILENAME);
            ToolProfile profile = new()
            {
                ProjectName = "1",
                WorkingDirectory = "C:\\1"
            };

            Assert.ThrowsAsync<InvalidOperationException>(async () => await configManager.AddProfile(profile));
        }


        [Test]
        public async Task RemoveProfile_RemovesProfile()
        {
            File.Copy("Data/config_with_multiple_profiles.json", Constants.CONFIG_FILENAME);

            ToolProfile profile = new()
            {
                ProjectName = "1",
                WorkingDirectory = "C:\\1"
            };
            await configManager.RemoveProfile(profile);
            var config = await configManager.GetConfig();

            Assert.That(config.Profiles, Has.Count.EqualTo(1));
        }


        [Test]
        public async Task GetDefaultInstallOptions_GetsCustomOptions()
        {
            File.Copy("Data/config_with_installoptions.json", Constants.CONFIG_FILENAME);

            var dbOptions = await configManager.GetDefaultInstallDatabaseOptions();
            var projectOptions = await configManager.GetDefaultInstallProjectOptions();

            Assert.Multiple(() =>
            {
                Assert.That(dbOptions.DatabaseName, Is.EqualTo("mydb"));
                Assert.That(dbOptions.ServerName, Is.EqualTo("myserver"));
                Assert.That(projectOptions.UseCloud, Is.True);
                Assert.That(projectOptions.ProjectName, Is.EqualTo("myproject"));
                Assert.That(projectOptions.Template, Is.EqualTo("kentico-xperience-sample-mvc"));
            });
        }
    }
}
