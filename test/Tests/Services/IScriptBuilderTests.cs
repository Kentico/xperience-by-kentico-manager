using NUnit.Framework;

using Xperience.Xman.Options;
using Xperience.Xman.Services;

namespace Xperience.Xman.Tests.Tests.Services
{
    /// <summary>
    /// Tests for <see cref="IScriptBuilder"/>.
    /// </summary>
    public class IScriptBuilderTests
    {
        private readonly IScriptBuilder scriptBuilder = new ScriptBuilder();
        private readonly InstallOptions validInstallOptions = new()
        {
            ProjectName = "TEST",
            AdminPassword = "PW",
            DatabaseName = "DB",
            ServerName = "TESTSERVER",
            Template = "kentico-xperience-sample-mvc"
        };
        private readonly UpdateOptions validUpdateOptions = new() { PackageName = "kentico.xperience.webapp" };


        [Test]
        public void ProjectInstallScript_WithValidOptions_ReturnsValidScript()
        {
            string script = scriptBuilder.SetScript(ScriptType.ProjectInstall).WithPlaceholders(validInstallOptions).Build();
            string expected = $"dotnet new {validInstallOptions.Template} -n {validInstallOptions.ProjectName}";

            Assert.That(script, Is.EqualTo(expected));
        }


        [Test]
        public void ProjectInstallScript_WithInvalidOptions_ThrowsException()
        {
            var options = new InstallOptions { Template = string.Empty };
            var builder = scriptBuilder.SetScript(ScriptType.ProjectInstall).WithPlaceholders(options);

            Assert.That(() => builder.Build(), Throws.InvalidOperationException);
        }


        [Test]
        public void TemplateInstall_AppendVersion_AddsParameter()
        {
            var version = new Version(1, 0, 0);
            string script = scriptBuilder.SetScript(ScriptType.TemplateInstall)
                .WithPlaceholders(validInstallOptions)
                .AppendVersion(version)
                .Build();
            string expected = $"dotnet new install kentico.xperience.templates::{version}";

            Assert.That(script, Is.EqualTo(expected));
        }


        [Test]
        public void PackageUpdate_AppendVersion_AddsParameter()
        {
            var version = new Version(1, 0, 0);
            string script = scriptBuilder.SetScript(ScriptType.PackageUpdate)
                .WithPlaceholders(validUpdateOptions)
                .AppendVersion(version)
                .Build();
            string expected = $"dotnet add package {validUpdateOptions.PackageName} --version {version}";

            Assert.That(script, Is.EqualTo(expected));
        }


        [Test]
        public void DatabaseInstallScript_WithValidOptions_ReturnsValidScript()
        {
            string script = scriptBuilder.SetScript(ScriptType.DatabaseInstall).WithPlaceholders(validInstallOptions).Build();
            string expected = $"dotnet kentico-xperience-dbmanager -- -s \"{validInstallOptions.ServerName}\" -d \"{validInstallOptions.DatabaseName}\" -a \"{validInstallOptions.AdminPassword}\"";

            Assert.That(script, Is.EqualTo(expected));
        }


        [Test]
        public void DatabaseInstallScript_WithInvalidOptions_ThrowsException()
        {
            var options = new InstallOptions();
            var builder = scriptBuilder.SetScript(ScriptType.DatabaseInstall).WithPlaceholders(options);

            Assert.That(() => builder.Build(), Throws.InvalidOperationException);
        }
    }
}
