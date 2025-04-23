using NSubstitute;

using NUnit.Framework;

using Xperience.Manager.Commands;
using Xperience.Manager.Options;
using Xperience.Manager.Services;
using Xperience.Manager.Wizards;

namespace Xperience.Manager.Tests.Commands
{
    /// <summary>
    /// Tests for <see cref="MacroCommand"/>.
    /// </summary>
    public class MacroCommandTests : TestBase
    {
        private const string USER = "admin";
        private const string OLD_SALT = "old";
        private const string NEW_SALT = "new";
        private readonly IShellRunner shellRunner = Substitute.For<IShellRunner>();
        private readonly IWizard<MacroOptions> macroWizard = Substitute.For<IWizard<MacroOptions>>();


        [SetUp]
        public void MacroCommandTestsSetUp() => shellRunner.Execute(Arg.Any<ShellOptions>()).Returns((x) => GetDummyProcess());


        [Test]
        public async Task Execute_SignAll_CallsSignAllScript()
        {
            macroWizard.Run().Returns(new MacroOptions
            {
                SignAll = true,
                UserName = USER,
                NewSalt = NEW_SALT
            });
            var command = new MacroCommand(macroWizard, shellRunner, new ScriptBuilder());
            await command.PreExecute(new(), string.Empty);
            await command.Execute(new(), string.Empty);

            string expectedMacroScript = $"dotnet run --no-build -- --kxp-resign-macros --sign-all --username \"{USER}\" --new-salt " +
                $"\"{NEW_SALT}\"";

            shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedMacroScript)));
        }


        [Test]
        public async Task Execute_OldAndNewSalt_ScriptContainsSalts()
        {
            macroWizard.Run().Returns(new MacroOptions
            {
                OldSalt = OLD_SALT,
                NewSalt = NEW_SALT
            });
            var command = new MacroCommand(macroWizard, shellRunner, new ScriptBuilder());
            await command.PreExecute(new(), string.Empty);
            await command.Execute(new(), string.Empty);

            string expectedMacroScript = $"dotnet run --no-build -- --kxp-resign-macros --old-salt \"{OLD_SALT}\" --new-salt " +
                $"\"{NEW_SALT}\"";

            shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedMacroScript)));
        }
    }
}
