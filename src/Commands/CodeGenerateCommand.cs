using Spectre.Console;

using Xperience.Manager.Configuration;
using Xperience.Manager.Options;
using Xperience.Manager.Services;
using Xperience.Manager.Wizards;

namespace Xperience.Manager.Commands
{
    /// <summary>
    /// A command which generates code files for Xperience by Kentico object types.
    /// </summary>
    public class CodeGenerateCommand : AbstractCommand
    {
        private readonly IShellRunner shellRunner;
        private readonly IScriptBuilder scriptBuilder;
        private readonly IWizard<CodeGenerateOptions> wizard;


        public override IEnumerable<string> Keywords => ["g", "generate"];


        public override IEnumerable<string> Parameters => [];


        public override string Description => "Generates code files for Xperience objects";


        public override bool RequiresProfile => true;


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal CodeGenerateCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public CodeGenerateCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder, IWizard<CodeGenerateOptions> wizard)
        {
            this.wizard = wizard;
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
        }


        public override async Task Execute(ToolProfile? profile, string? action)
        {
            var options = await wizard.Run();
            AnsiConsole.WriteLine();
            await GenerateCode(profile, options);
        }


        public override async Task PostExecute(ToolProfile? profile, string? action)
        {
            if (!Errors.Any())
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Code generation complete![/]\n");
            }

            await base.PostExecute(profile, action);
        }


        private async Task GenerateCode(ToolProfile? profile, CodeGenerateOptions options)
        {
            if (StopProcessing)
            {
                return;
            }

            // Add working directory to relative location
            options.Location = profile?.WorkingDirectory + options.Location;

            string buildScript = scriptBuilder.SetScript(ScriptType.GenerateCode).WithPlaceholders(options).AppendNamespace(options.Namespace).Build();
            await shellRunner.Execute(new(buildScript)
            {
                OutputHandler = (o, e) =>
                {
                    if (e.Data?.Contains("Generating code files for", StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]{e.Data}[/]");
                    }
                },
                ErrorHandler = ErrorDataReceived,
                WorkingDirectory = profile?.WorkingDirectory
            }).WaitForExitAsync();
        }
    }
}
