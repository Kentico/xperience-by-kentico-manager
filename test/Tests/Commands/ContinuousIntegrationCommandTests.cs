using NSubstitute;

using NUnit.Framework;

using Xperience.Manager.Commands;
using Xperience.Manager.Configuration;
using Xperience.Manager.Services;

namespace Xperience.Manager.Tests.Commands
{
    /// <summary>
    /// Tests for <see cref="ContinuousIntegrationCommand"/>.
    /// </summary>
    public class ContinuousIntegrationCommandTests : TestBase
    {
        private const string PROJECT = "myproj";
        private readonly IShellRunner shellRunner = Substitute.For<IShellRunner>();
        private readonly ToolProfile profile = new()
        {
            ProjectName = PROJECT
        };


        [SetUp]
        public void ContinuousIntegrationCommandTestsSetUp() => shellRunner.Execute(Arg.Any<ShellOptions>()).Returns((x) =>
            GetDummyProcess());


        [Test]
        public async Task Execute_StoreParameter_CallsStoreScript()
        {
            var command = new ContinuousIntegrationCommand(shellRunner, new ScriptBuilder());
            await command.PreExecute(profile, "store");
            await command.Execute(profile, "store");

            string expectedScript = $"dotnet run --project \"{PROJECT}.csproj\" --no-build --kxp-ci-store";

            shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedScript)));
        }


        [Test]
        public async Task Execute_RestoreParameter_CallsRestoreScript()
        {
            var command = new ContinuousIntegrationCommand(shellRunner, new ScriptBuilder());
            await command.PreExecute(profile, "restore");
            await command.Execute(profile, "restore");

            string expectedScript = $"dotnet run --project \"{PROJECT}.csproj\" --no-build --kxp-ci-restore";

            shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedScript)));
        }
    }
}
