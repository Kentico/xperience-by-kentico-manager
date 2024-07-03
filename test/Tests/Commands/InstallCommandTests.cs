using NSubstitute;

using NUnit.Framework;

using Xperience.Manager.Commands;
using Xperience.Manager.Options;
using Xperience.Manager.Services;
using Xperience.Manager.Wizards;

namespace Xperience.Manager.Tests.Commands
{
    /// <summary>
    /// Tests for <see cref="InstallCommand"/>.
    /// </summary>
    public class InstallCommandTests : TestBase
    {
        private const string DB_NAME = "TESTDB";
        private const string PASSWORD = "PW";
        private const string SERVER_NAME = "TESTSERVER";
        private const string TEMPLATE = "TEMPLATE";
        private const string PROJECT_NAME = "PROJECT";
        private const bool USE_EXISTING = false;
        private readonly Version version = new(1, 0, 0);
        private readonly IShellRunner shellRunner = Substitute.For<IShellRunner>();
        private readonly IWizard<InstallOptions> installWizard = Substitute.For<IWizard<InstallOptions>>();


        [SetUp]
        public void InstallCommandTestsSetUp()
        {
            installWizard.Run().Returns(new InstallOptions
            {
                ProjectName = PROJECT_NAME,
                AdminPassword = PASSWORD,
                DatabaseName = DB_NAME,
                ServerName = SERVER_NAME,
                Version = version,
                Template = TEMPLATE,
                UseExistingDatabase = USE_EXISTING
            });

            shellRunner.Execute(Arg.Any<ShellOptions>()).Returns((x) => GetDummyProcess());
        }


        [Test]
        public async Task Execute_CallsInstallationScripts()
        {
            var command = new InstallCommand(shellRunner, new ScriptBuilder(), installWizard, Substitute.For<IConfigManager>());
            await command.PreExecute(new(), string.Empty);
            await command.Execute(new(), string.Empty);

            string expectedProjectFileScript = $"dotnet new {TEMPLATE} -n {PROJECT_NAME}";
            string expectedUninstallScript = "dotnet new uninstall kentico.xperience.templates";
            string expectedTemplateScript = $"dotnet new install kentico.xperience.templates::{version}";
            string expectedDatabaseScript = $"dotnet kentico-xperience-dbmanager -- -s \"{SERVER_NAME}\" -d \"{DB_NAME}\" -a \"{PASSWORD}\" --use-existing-database {USE_EXISTING}";

            Assert.Multiple(() =>
            {
                shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedProjectFileScript)));
                shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedUninstallScript)));
                shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedTemplateScript)));
                shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedDatabaseScript)));
            });
        }
    }
}
