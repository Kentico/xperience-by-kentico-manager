using Xperience.Manager.Configuration;
using Xperience.Manager.Options;

namespace Xperience.Manager.Services
{
    public class ScriptBuilder : IScriptBuilder
    {
        private ScriptType currentScriptType;
        private string currentScript = string.Empty;

        private const string BUILD_SCRIPT = "dotnet build";
        private const string MKDIR_SCRIPT = $"mkdir";
        private const string INSTALL_PROJECT_SCRIPT = $"dotnet new {nameof(InstallOptions.Template)} -n {nameof(InstallOptions.ProjectName)}";
        private const string INSTALL_DATABASE_SCRIPT = $"dotnet kentico-xperience-dbmanager -- -s \"{nameof(InstallOptions.ServerName)}\" -d \"{nameof(InstallOptions.DatabaseName)}\" -a \"{nameof(InstallOptions.AdminPassword)}\"";
        private const string UNINSTALL_TEMPLATE_SCRIPT = "dotnet new uninstall kentico.xperience.templates";
        private const string INSTALL_TEMPLATE_SCRIPT = "dotnet new install kentico.xperience.templates";
        private const string UPDATE_PACKAGE_SCRIPT = $"dotnet add package {nameof(UpdateOptions.PackageName)}";
        private const string UPDATE_DATABASE_SCRIPT = "dotnet run --no-build --kxp-update -- --skip-confirmation";
        private const string CI_STORE_SCRIPT = "dotnet run --no-build --kxp-ci-store";
        private const string CI_RESTORE_SCRIPT = "dotnet run --no-build --kxp-ci-restore";
        private const string CD_NEW_CONFIG_SCRIPT = $"dotnet run --no-build -- --kxp-cd-config --path \"{nameof(ContinuousDeploymentConfig.ConfigPath)}\"";
        private const string CD_STORE_SCRIPT = $"dotnet run --no-build -- --kxp-cd-store --repository-path \"{nameof(ContinuousDeploymentConfig.RepositoryPath)}\" --config-path \"{nameof(ContinuousDeploymentConfig.ConfigPath)}\"";
        private const string CD_RESTORE_SCRIPT = $"dotnet run -- --kxp-cd-restore --repository-path \"{nameof(ContinuousDeploymentConfig.RepositoryPath)}\"";
        private const string MACRO_SCRIPT = "dotnet run --no-build -- --kxp-resign-macros";
        private const string CODEGEN_SCRIPT = $"dotnet run -- --kxp-codegen --skip-confirmation --type \"{nameof(CodeGenerateOptions.Type)}\" --location \"{nameof(CodeGenerateOptions.Location)}\" --include \"{nameof(CodeGenerateOptions.Include)}\" --exclude \"{nameof(CodeGenerateOptions.Exclude)}\" --with-provider-class {nameof(CodeGenerateOptions.WithProviderClass)}";
        private const string DELETE_FOLDER_SCRIPT = $"rm \"{nameof(ToolProfile.WorkingDirectory)}\" -r -Force";
        private const string RUN_SQL_QUERY = $"Invoke-Sqlcmd -ConnectionString \"{nameof(RunSqlOptions.ConnString)}\" -Query \"{nameof(RunSqlOptions.SqlQuery)}\"";


        public IScriptBuilder AppendCloud(bool useCloud)
        {
            if (currentScriptType.Equals(ScriptType.ProjectInstall) && useCloud)
            {
                currentScript += " --cloud";
            }

            return this;
        }


        public IScriptBuilder AppendDirectory(string? path)
        {
            if (currentScriptType.Equals(ScriptType.CreateDirectory))
            {
                currentScript += $" \"{path}\"";
            }

            return this;
        }


        public IScriptBuilder AppendNamespace(string? nameSpace)
        {
            if (currentScriptType.Equals(ScriptType.GenerateCode) && !string.IsNullOrEmpty(nameSpace))
            {
                currentScript += $" --namespace \"{nameSpace}\"";
            }

            return this;
        }


        public IScriptBuilder AppendSalt(string? salt, bool isOld)
        {
            if (string.IsNullOrEmpty(salt) || !currentScriptType.Equals(ScriptType.ResignMacros))
            {
                return this;
            }

            currentScript += $" {(isOld ? "--old-salt" : "--new-salt")} \"{salt}\"";

            return this;
        }


        public IScriptBuilder AppendSignAll(bool signAll, string? userName)
        {
            if (!signAll || string.IsNullOrEmpty(userName) || !currentScriptType.Equals(ScriptType.ResignMacros))
            {
                return this;
            }

            currentScript += $" --sign-all --username \"{userName}\"";

            return this;
        }


        public IScriptBuilder AppendVersion(Version? version)
        {
            if (version is null)
            {
                return this;
            }

            if (currentScriptType.Equals(ScriptType.TemplateInstall))
            {
                currentScript += $"::{version}";
            }
            else if (currentScriptType.Equals(ScriptType.PackageUpdate))
            {
                currentScript += $" --version {version}";
            }

            return this;
        }


        public string Build()
        {
            if (!ValidateScript())
            {
                throw new InvalidOperationException("The script is empty or contains placeholder values.");
            }

            return currentScript;
        }


        public IScriptBuilder WithPlaceholders(object? dataObject)
        {
            if (dataObject is null)
            {
                return this;
            }

            // Replace all placeholders in script with object values if non-null or empty
            foreach (var prop in dataObject.GetType().GetProperties())
            {
                string value = prop.GetValue(dataObject)?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(value))
                {
                    currentScript = currentScript.Replace(prop.Name, value);
                }
            }

            return this;
        }


        public IScriptBuilder SetScript(ScriptType type)
        {
            if (type.Equals(ScriptType.None))
            {
                throw new InvalidOperationException("Invalid script type.");
            }

            currentScriptType = type;
            currentScript = type switch
            {
                ScriptType.BuildProject => BUILD_SCRIPT,
                ScriptType.CreateDirectory => MKDIR_SCRIPT,
                ScriptType.ProjectInstall => INSTALL_PROJECT_SCRIPT,
                ScriptType.DatabaseInstall => INSTALL_DATABASE_SCRIPT,
                ScriptType.TemplateUninstall => UNINSTALL_TEMPLATE_SCRIPT,
                ScriptType.TemplateInstall => INSTALL_TEMPLATE_SCRIPT,
                ScriptType.PackageUpdate => UPDATE_PACKAGE_SCRIPT,
                ScriptType.DatabaseUpdate => UPDATE_DATABASE_SCRIPT,
                ScriptType.RestoreContinuousIntegration => CI_RESTORE_SCRIPT,
                ScriptType.StoreContinuousIntegration => CI_STORE_SCRIPT,
                ScriptType.ContinuousDeploymentNewConfiguration => CD_NEW_CONFIG_SCRIPT,
                ScriptType.ContinuousDeploymentStore => CD_STORE_SCRIPT,
                ScriptType.ContinuousDeploymentRestore => CD_RESTORE_SCRIPT,
                ScriptType.ResignMacros => MACRO_SCRIPT,
                ScriptType.GenerateCode => CODEGEN_SCRIPT,
                ScriptType.DeleteDirectory => DELETE_FOLDER_SCRIPT,
                ScriptType.ExecuteSql => RUN_SQL_QUERY,
                ScriptType.None => string.Empty,
                _ => string.Empty,
            };

            return this;
        }


        private bool ValidateScript()
        {
            var propertyNames = typeof(InstallOptions).GetProperties().Select(p => p.Name);

            return !string.IsNullOrEmpty(currentScript) && !propertyNames.Any(currentScript.Contains);
        }
    }


    public enum ScriptType
    {
        /// <summary>
        /// An invalid script type.
        /// </summary>
        None,


        /// <summary>
        /// The script which installs new Xperience by Kentico project files.
        /// </summary>
        ProjectInstall,


        /// <summary>
        /// The script which installs a new Xperience by Kentico database.
        /// </summary>
        DatabaseInstall,


        /// <summary>
        /// The script which uninstalls the Xperience by Kentico templates.
        /// </summary>
        TemplateUninstall,


        /// <summary>
        /// The script which installs the Xperience by Kentico templates.
        /// </summary>
        TemplateInstall,


        /// <summary>
        /// The script which updates the Xperience by Kentico packages.
        /// </summary>
        PackageUpdate,


        /// <summary>
        /// The script which updates the Xperience by Kentico database.
        /// </summary>
        DatabaseUpdate,


        /// <summary>
        /// The script which builds the project.
        /// </summary>
        BuildProject,


        /// <summary>
        /// The script which stores Continuous Intgeration data on the filesystem.
        /// </summary>
        StoreContinuousIntegration,


        /// <summary>
        /// The script which restores Continuous Intgeration data to the database.
        /// </summary>
        RestoreContinuousIntegration,


        /// <summary>
        /// The script which creates a new directory.
        /// </summary>
        CreateDirectory,


        /// <summary>
        /// The script which creates a new Continuous Deployment configuration file.
        /// </summary>
        ContinuousDeploymentNewConfiguration,


        /// <summary>
        /// The script which stores Continuous Deployment data on the filesystem.
        /// </summary>
        ContinuousDeploymentStore,


        /// <summary>
        /// The script which restores Continuous Deployment data to the database.
        /// </summary>
        ContinuousDeploymentRestore,


        /// <summary>
        /// The script which re-signs macros.
        /// </summary>
        ResignMacros,

        /// <summary>
        /// The script which generates code files for Xperience objects.
        /// </summary>
        GenerateCode,

        /// <summary>
        /// The script which deletes a local folder and its contents.
        /// </summary>
        DeleteDirectory,


        /// <summary>
        /// The script which executes a SQL query against a database.
        /// </summary>
        ExecuteSql,
    }
}
