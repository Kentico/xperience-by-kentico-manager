using NSubstitute;

using NUnit.Framework;

using Xperience.Manager.Commands;
using Xperience.Manager.Services;

namespace Xperience.Manager.Tests.Commands
{
    /// <summary>
    /// Tests for <see cref="BuildCommand"/>.
    /// </summary>
    public class BuildCommandTests : TestBase
    {
        private readonly IShellRunner shellRunner = Substitute.For<IShellRunner>();


        [SetUp]
        public void BuildCommandTestsSetUp() => shellRunner.Execute(Arg.Any<ShellOptions>()).Returns((x) => GetDummyProcess());


        [Test]
        public async Task Execute_CallsBuildScript()
        {
            var command = new BuildCommand(shellRunner, new ScriptBuilder());
            await command.PreExecute(new(), string.Empty);
            await command.Execute(new(), string.Empty);

            shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals($"dotnet build")));
        }
    }
}
