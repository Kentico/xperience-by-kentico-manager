using System.Diagnostics;

using Spectre.Console;

using Xperience.Xman.Configuration;
using Xperience.Xman.Options;
using Xperience.Xman.Services;
using Xperience.Xman.Wizards;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// A command which manages Continuous Deployment profiles and stores/restores data.
    /// </summary>
    public class ContinuousDeploymentCommand : AbstractCommand
    {
        private const string STORE = "store";
        private const string RESTORE = "restore";
        private const string CONFIG = "config";
        private readonly IShellRunner shellRunner;
        private readonly IScriptBuilder scriptBuilder;
        private readonly IConfigManager configManager;
        private readonly ICDXmlManager cdXmlManager;
        private readonly IWizard<RepositoryConfiguration> wizard;


        public override IEnumerable<string> Keywords => new string[] { "cd" };


        public override IEnumerable<string> Parameters => new string[] { STORE, RESTORE, CONFIG };


        public override string Description => "Stores or restores CD data, or edits the config file";


        public override bool RequiresProfile => true;


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal ContinuousDeploymentCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public ContinuousDeploymentCommand(IWizard<RepositoryConfiguration> wizard, ICDXmlManager cdXmlManager, IShellRunner shellRunner, IScriptBuilder scriptBuilder, IConfigManager configManager)
        {
            this.wizard = wizard;
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
            this.configManager = configManager;
            this.cdXmlManager = cdXmlManager;
        }


        public override async Task PreExecute(ToolProfile? profile, string? action)
        {
            if (string.IsNullOrEmpty(action) || !Parameters.Any(p => p.Equals(action, StringComparison.OrdinalIgnoreCase)))
            {
                LogError($"Must provide one parameter from '{string.Join(", ", Parameters)}'");
            }

            await base.PreExecute(profile, action);
        }


        public override async Task Execute(ToolProfile? profile, string? action)
        {
            if (StopProcessing)
            {
                return;
            }

            var config = await configManager.GetConfig();
            await EnsureCDStructure(profile, config);
            if (action?.Equals(CONFIG, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                await ConfigureXml(config, profile);
            }
            else if (action?.Equals(STORE, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                await AnsiConsole.Progress()
                    .Columns(new ProgressColumn[]
                    {
                        new SpinnerColumn(),
                        new ElapsedTimeColumn(),
                        new TaskDescriptionColumn(),
                        new ProgressBarColumn(),
                        new PercentageColumn()
                    })
                    .StartAsync(async ctx =>
                    {
                        var task = ctx.AddTask($"[{Constants.EMPHASIS_COLOR}]Running the CD store script[/]");
                        await StoreFiles(task, profile, config);
                    });
            }
            else if (action?.Equals(RESTORE, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                var sourceProfile = GetSourceProfile(profile, config);
                if (sourceProfile is null)
                {
                    return;
                }

                await AnsiConsole.Progress()
                    .Columns(new ProgressColumn[]
                    {
                        new SpinnerColumn(),
                        new ElapsedTimeColumn(),
                        new TaskDescriptionColumn()
                    })
                    .StartAsync(async ctx =>
                    {
                        var task = ctx.AddTask($"[{Constants.EMPHASIS_COLOR}]Running the CD restore script[/]");
                        await RestoreFiles(task, profile, sourceProfile, config);
                    });
            }
        }


        public override async Task PostExecute(ToolProfile? profile, string? action)
        {
            if (!Errors.Any())
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]CD {action ?? "process"} complete![/]\n");
            }

            await base.PostExecute(profile, action);
        }


        private async Task ConfigureXml(ToolConfiguration toolConfig, ToolProfile? profile)
        {
            if (StopProcessing)
            {
                return;
            }

            if (string.IsNullOrEmpty(profile?.ProjectName))
            {
                LogError("Unable to load profile name.");
                return;
            }

            ContinuousDeploymentConfig cdConfig = new()
            {
                ConfigPath = Path.Combine(toolConfig.CDRootPath, profile.ProjectName, Constants.CD_CONFIG_NAME),
                RepositoryPath = Path.Combine(toolConfig.CDRootPath, profile.ProjectName, Constants.CD_FILES_DIR)
            };

            var repoConfig = await cdXmlManager.GetConfig(cdConfig.ConfigPath);
            if (repoConfig is null)
            {
                LogError("Unable to read repository configuration.");
                return;
            }

            wizard.Options = repoConfig;
            var options = await wizard.Run();
            cdXmlManager.WriteConfig(options, cdConfig.ConfigPath);
        }


        private ToolProfile? GetSourceProfile(ToolProfile? profile, ToolConfiguration config)
        {
            if (StopProcessing)
            {
                return null;
            }

            var profiles = config.Profiles.Where(p => !p.ProjectName?.Equals(profile?.ProjectName, StringComparison.OrdinalIgnoreCase) ?? false);
            if (!profiles.Any())
            {
                LogError("There are no profiles to restore CD data from. Use the 'install' or 'profile add' commands to register a new profile.");
                return null;
            }

            var prompt = new SelectionPrompt<ToolProfile>()
                    .Title("Restore data from which profile?")
                    .PageSize(10)
                    .UseConverter(p => p.ProjectName ?? string.Empty)
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(profiles);

            return AnsiConsole.Prompt(prompt);
        }


        private async Task EnsureCDStructure(ToolProfile? profile, ToolConfiguration config)
        {
            if (StopProcessing)
            {
                return;
            }

            if (string.IsNullOrEmpty(profile?.ProjectName))
            {
                LogError("Unable to load profile name.");
                return;
            }

            ContinuousDeploymentConfig cdConfig = new()
            {
                ConfigPath = Path.Combine(config.CDRootPath, profile.ProjectName, Constants.CD_CONFIG_NAME),
                RepositoryPath = Path.Combine(config.CDRootPath, profile.ProjectName, Constants.CD_FILES_DIR)
            };

            Directory.CreateDirectory(cdConfig.RepositoryPath);

            if (!File.Exists(cdConfig.ConfigPath))
            {
                string cdScript = scriptBuilder.SetScript(ScriptType.ContinuousDeploymentNewConfiguration)
                    .WithPlaceholders(cdConfig)
                    .Build();
                await shellRunner.Execute(new(cdScript)
                {
                    ErrorHandler = ErrorDataReceived,
                    WorkingDirectory = profile.WorkingDirectory
                }).WaitForExitAsync();
            }
        }


        private async Task RestoreFiles(ProgressTask task, ToolProfile? profile, ToolProfile sourceProfile, ToolConfiguration config)
        {
            if (StopProcessing)
            {
                return;
            }

            if (string.IsNullOrEmpty(sourceProfile.ProjectName))
            {
                LogError("Unable to load profile name.");
                return;
            }

            ContinuousDeploymentConfig cdConfig = new()
            {
                ConfigPath = Path.Combine(config.CDRootPath, sourceProfile.ProjectName, Constants.CD_CONFIG_NAME),
                RepositoryPath = Path.Combine(config.CDRootPath, sourceProfile.ProjectName, Constants.CD_FILES_DIR)
            };

            string originalDescription = task.Description;
            string cdScript = scriptBuilder.SetScript(ScriptType.ContinuousDeploymentRestore)
                .WithPlaceholders(cdConfig)
                .Build();
            await shellRunner.Execute(new(cdScript)
            {
                ErrorHandler = ErrorDataReceived,
                WorkingDirectory = profile?.WorkingDirectory,
                OutputHandler = (o, e) =>
                {
                    if (e.Data?.Contains("Object type", StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        // Message is something like "Object type Module: updating Activities"
                        task.Description = e.Data;
                    }
                }
            }).WaitForExitAsync();

            task.Description = originalDescription;
        }


        private async Task StoreFiles(ProgressTask task, ToolProfile? profile, ToolConfiguration config)
        {
            if (StopProcessing)
            {
                return;
            }

            if (string.IsNullOrEmpty(profile?.ProjectName))
            {
                LogError("Unable to load profile name.");
                return;
            }

            ContinuousDeploymentConfig cdConfig = new()
            {
                ConfigPath = Path.Combine(config.CDRootPath, profile.ProjectName, Constants.CD_CONFIG_NAME),
                RepositoryPath = Path.Combine(config.CDRootPath, profile.ProjectName, Constants.CD_FILES_DIR)
            };
            string cdScript = scriptBuilder.SetScript(ScriptType.ContinuousDeploymentStore)
                .WithPlaceholders(cdConfig)
                .Build();
            await shellRunner.Execute(new(cdScript)
            {
                ErrorHandler = ErrorDataReceived,
                OutputHandler = (o, e) =>
                {
                    if (e.Data?.Contains("System.IO.IOException", StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        // For some reason, System.IO.IOException is not caught by the error handler
                        LogError(e.Data, o as Process);
                    }
                    else if ((e.Data?.Contains("Object type", StringComparison.OrdinalIgnoreCase) ?? false) && e.Data.Any(char.IsDigit))
                    {
                        // Message is something like "Object type 1/84: Module"
                        string[] progressMessage = e.Data.Split(':');
                        if (progressMessage.Length == 0)
                        {
                            return;
                        }

                        string[] progressNumbers = progressMessage[0].Split('/');
                        if (progressNumbers.Length < 2)
                        {
                            return;
                        }

                        double progressCurrent = double.Parse(string.Join("", progressNumbers[0].Where(char.IsDigit)));
                        double progressMax = double.Parse(string.Join("", progressNumbers[1].Where(char.IsDigit)));

                        task.MaxValue = progressMax;
                        task.Value = progressCurrent;
                    }
                },
                WorkingDirectory = profile.WorkingDirectory
            }).WaitForExitAsync();
        }
    }
}
