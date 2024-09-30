using NUnit.Framework;

using Xperience.Manager.Options;
using Xperience.Manager.Services;

namespace Xperience.Manager.Tests.Services
{
    /// <summary>
    /// Tests for <see cref="IScriptBuilder"/>.
    /// </summary>
    public class IScriptBuilderTests
    {
        private readonly IScriptBuilder scriptBuilder = new ScriptBuilder();
        private readonly InstallProjectOptions validProjectOptions = new()
        {
            ProjectName = "TEST",
            Template = "kentico-xperience-sample-mvc"
        };
        private readonly InstallDatabaseOptions validDatabaseOptions = new()
        {
            AdminPassword = "PW",
            DatabaseName = "DB",
            ServerName = "SERVER"
        };
        private readonly UpdateOptions validUpdateOptions = new() { PackageName = "kentico.xperience.webapp" };


        [Test]
        public void ProjectInstallScript_WithValidOptions_ReturnsValidScript()
        {
            string script = scriptBuilder.SetScript(ScriptType.ProjectInstall).WithPlaceholders(validProjectOptions).Build();
            string expected = $"dotnet new {validProjectOptions.Template} -n {validProjectOptions.ProjectName}";

            Assert.That(script, Is.EqualTo(expected));
        }


        [Test]
        public void TemplateInstall_AppendVersion_AddsParameter()
        {
            var version = new Version(1, 0, 0);
            string script = scriptBuilder.SetScript(ScriptType.TemplateInstall)
                .WithPlaceholders(validProjectOptions)
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
            string script = scriptBuilder.SetScript(ScriptType.DatabaseInstall).WithPlaceholders(validDatabaseOptions).Build();
            string expected = $"dotnet kentico-xperience-dbmanager -- -s \"{validDatabaseOptions.ServerName}\" -d \"{validDatabaseOptions.DatabaseName}\" -a \"{validDatabaseOptions.AdminPassword}\" --use-existing-database {validDatabaseOptions.UseExistingDatabase}";

            Assert.That(script, Is.EqualTo(expected));
        }
    }
}
