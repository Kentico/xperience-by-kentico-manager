using NSubstitute;

using NUnit.Framework;

using Xperience.Manager.Commands;
using Xperience.Manager.Configuration;
using Xperience.Manager.Options;
using Xperience.Manager.Services;
using Xperience.Manager.Wizards;

namespace Xperience.Manager.Tests.Commands
{
    /// <summary>
    /// Tests for <see cref="CodeGenerateCommand"/>.
    /// </summary>
    public class CodeGenerateCommandTests : TestBase
    {
        private const string EXCLUDE = "ex";
        private const string INCLUDE = "*";
        private const string LOCATION = "/dir";
        private const string NAMESPACE = "ns";
        private const string PROJECT_NAME = "myproj";
        private const string WORKING_DIR = "c:/mysite";
        private const string TYPE = CodeGenerateOptions.TYPE_REUSABLE_CONTENT_TYPES;
        private const bool WITH_PROVIDER = false;
        private readonly IShellRunner shellRunner = Substitute.For<IShellRunner>();
        private readonly IWizard<CodeGenerateOptions> generateWizard = Substitute.For<IWizard<CodeGenerateOptions>>();


        [SetUp]
        public void CodeGenerateCommandTestsSetUp()
        {
            generateWizard.Run().Returns(new CodeGenerateOptions
            {
                Exclude = EXCLUDE,
                Include = INCLUDE,
                Location = LOCATION,
                Namespace = NAMESPACE,
                Type = TYPE,
                WithProviderClass = WITH_PROVIDER
            });

            shellRunner.Execute(Arg.Any<ShellOptions>()).Returns((x) => GetDummyProcess());
        }


        [Test]
        public async Task Execute_CallsCodeGenScript()
        {
            ToolProfile profile = new()
            {
                ProjectName = PROJECT_NAME,
                WorkingDirectory = WORKING_DIR
            };
            var command = new CodeGenerateCommand(shellRunner, new ScriptBuilder(), generateWizard);
            await command.PreExecute(profile, string.Empty);
            await command.Execute(profile, string.Empty);

            string expectedCodeGenScript = $"dotnet run --project \"{PROJECT_NAME}.csproj\" --no-build -- --kxp-codegen " +
                $"--skip-confirmation --type \"{TYPE}\" --location \"{WORKING_DIR}{LOCATION}\" --include \"{INCLUDE}\" " +
                $"--exclude \"{EXCLUDE}\" --with-provider-class {WITH_PROVIDER} --namespace \"{NAMESPACE}\"";

            shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedCodeGenScript)));
        }
    }
}
