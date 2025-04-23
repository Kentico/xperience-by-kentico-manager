using NSubstitute;

using NUnit.Framework;

using Xperience.Manager.Commands;
using Xperience.Manager.Options;
using Xperience.Manager.Services;
using Xperience.Manager.Wizards;

namespace Xperience.Manager.Tests.Commands
{
    /// <summary>
    /// Tests for <see cref="UpdateCommand"/>.
    /// </summary>
    public class UpdateCommandTests : TestBase
    {
        private readonly Version version = new(1, 0, 0);
        private readonly IShellRunner shellRunner = Substitute.For<IShellRunner>();
        private readonly IWizard<UpdateOptions> updateWizard = Substitute.For<IWizard<UpdateOptions>>();


        [SetUp]
        public void UpdateCommandTestsSetUp()
        {
            updateWizard.Run().Returns(new UpdateOptions
            {
                Version = version
            });

            shellRunner.Execute(Arg.Any<ShellOptions>()).Returns((x) => GetDummyProcess());
        }


        [Test]
        public async Task Execute_CallsUpdateScripts()
        {
            var command = new UpdateCommand(shellRunner, new ScriptBuilder(), updateWizard);
            await command.PreExecute(new(), string.Empty);
            await command.Execute(new(), string.Empty);

            string[] packageNames =
            [
                "kentico.xperience.admin",
                "kentico.xperience.azurestorage",
                "kentico.xperience.cloud",
                "kentico.xperience.graphql",
                "kentico.xperience.imageprocessing",
                "kentico.xperience.webapp"
            ];

            foreach (string p in packageNames)
            {
                shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals($"dotnet add package {p} --version {version}")));
            }
        }
    }
}
