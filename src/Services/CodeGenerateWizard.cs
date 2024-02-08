using Spectre.Console;

using Xperience.Xman.Options;
using Xperience.Xman.Steps;

namespace Xperience.Xman.Wizards
{
    /// <summary>
    /// A wizard which generates an <see cref="CodeGenerateOptions"/> for generating code files.
    /// </summary>
    public class CodeGenerateWizard : AbstractWizard<CodeGenerateOptions>
    {
        private readonly IEnumerable<string> types = new string[]
        {
            CodeGenerateOptions.TYPE_PAGE_CONTENT_TYPES,
            CodeGenerateOptions.TYPE_REUSABLE_CONTENT_TYPES,
            CodeGenerateOptions.TYPE_REUSABLE_FIELD_SCHEMAS,
            CodeGenerateOptions.TYPE_CLASSES
        };


        public override Task InitSteps()
        {
            Steps.Add(new Step<string>(new()
            {
                Prompt = new SelectionPrompt<string>()
                    .Title($"Which [{Constants.PROMPT_COLOR}]type[/]?")
                    .PageSize(10)
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(types),
                ValueReceiver = (v) => Options.Type = v
            }));

            Steps.Add(new Step<bool>(new()
            {
                Prompt = new ConfirmationPrompt($"Generate [{Constants.PROMPT_COLOR}]provider classes[/]?")
                {
                    DefaultValue = Options.WithProviderClass
                },
                ValueReceiver = (v) => Options.WithProviderClass = v,
                SkipChecker = () => !Options.Type?.Equals(CodeGenerateOptions.TYPE_CLASSES) ?? true
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Use a custom [{Constants.PROMPT_COLOR}]namespace[/]?").AllowEmpty(),
                ValueReceiver = (v) => Options.Namespace = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Enter the relative [{Constants.PROMPT_COLOR}]location[/] to generate files:")
                    .DefaultValue(Options.Location)
                    .Validate((v) => v.StartsWith("/"))
                    .ValidationErrorMessage("Location must start with '/'"),
                ValueReceiver = (v) => Options.Location = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"[{Constants.PROMPT_COLOR}]Include[/] which object types (semicolon separated list)?").DefaultValue(Options.Include),
                ValueReceiver = (v) => Options.Include = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"[{Constants.PROMPT_COLOR}]Exclude[/] which object types (semicolon separated list)?").DefaultValue(Options.Exclude),
                ValueReceiver = (v) => Options.Exclude = v
            }));

            return Task.CompletedTask;
        }
    }
}
