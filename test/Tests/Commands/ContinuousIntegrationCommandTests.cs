using NSubstitute;

using NUnit.Framework;

using Xperience.Manager.Commands;
using Xperience.Manager.Services;

namespace Xperience.Manager.Tests.Commands
{
    /// <summary>
    /// Tests for <see cref="ContinuousIntegrationCommand"/>.
    /// </summary>
    public class ContinuousIntegrationCommandTests : TestBase
    {
        private readonly IShellRunner shellRunner = Substitute.For<IShellRunner>();


        [SetUp]
        public void ContinuousIntegrationCommandTestsSetUp() => shellRunner.Execute(Arg.Any<ShellOptions>()).Returns((x) => GetDummyProcess());


        [Test]
        public async Task Execute_StoreParameter_CallsStoreScript()
        {
            var command = new ContinuousIntegrationCommand(shellRunner, new ScriptBuilder());
            await command.PreExecute(new(), "store");
            await command.Execute(new(), "store");

            string expectedScript = "dotnet run --no-build --kxp-ci-store";

            shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedScript)));
        }


        [Test]
        public async Task Execute_RestoreParameter_CallsRestoreScript()
        {
            var command = new ContinuousIntegrationCommand(shellRunner, new ScriptBuilder());
            await command.PreExecute(new(), "restore");
            await command.Execute(new(), "restore");

            string expectedScript = "dotnet run --no-build --kxp-ci-restore";

            shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedScript)));
        }
    }
}
