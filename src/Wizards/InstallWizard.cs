using Spectre.Console;

using Xperience.Manager.Helpers;
using Xperience.Manager.Options;
using Xperience.Manager.Steps;

namespace Xperience.Manager.Wizards
{
    /// <summary>
    /// A wizard which generates an <see cref="InstallOptions"/> for installing Xperience by Kentico.
    /// </summary>
    public class InstallWizard : AbstractWizard<InstallOptions>
    {
        private readonly IEnumerable<string> templates = [
            Constants.TEMPLATE_SAMPLE,
            Constants.TEMPLATE_BLANK,
            Constants.TEMPLATE_ADMIN
        ];


        public override async Task InitSteps()
        {
            var versions = await NuGetVersionHelper.GetPackageVersions(Constants.TEMPLATES_PACKAGE);
            var filtered = versions.Where(v => !v.IsPrerelease && !v.IsLegacyVersion && v.Major >= 25)
                .Select(v => v.Version)
                .OrderByDescending(v => v);

            Steps.Add(new Step<Version>(new()
            {
                Prompt = new SelectionPrompt<Version>()
                    .Title($"Which [{Constants.PROMPT_COLOR}]version[/]?")
                    .PageSize(10)
                    .UseConverter(v => $"{v.Major}.{v.Minor}.{v.Build}")
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(filtered),
                ValueReceiver = (v) => Options.Version = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new SelectionPrompt<string>()
                    .Title($"Which [{Constants.PROMPT_COLOR}]template[/]?")
                    .AddChoices(templates),
                ValueReceiver = (v) => Options.Template = v.ToString()
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Give your project a [{Constants.PROMPT_COLOR}]name[/]:")
                    .DefaultValue(Options.ProjectName),
                ValueReceiver = (v) => Options.ProjectName = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Install [{Constants.PROMPT_COLOR}]where[/]?")
                    .DefaultValue(Options.InstallRootPath),
                ValueReceiver = (v) => Options.InstallRootPath = v
            }));

            var cloudPrompt = new ConfirmationPrompt($"Prepare for [{Constants.PROMPT_COLOR}]cloud[/] deployment?")
            {
                DefaultValue = Options.UseCloud
            };
            Steps.Add(new Step<bool>(new()
            {
                Prompt = cloudPrompt,
                ValueReceiver = (v) => Options.UseCloud = v,
                SkipChecker = IsAdminTemplate
            }));


            var serverPrompt = new TextPrompt<string>($"Enter the [{Constants.PROMPT_COLOR}]SQL server[/] name:");
            if (!string.IsNullOrEmpty(Options.ServerName))
            {
                serverPrompt.DefaultValue(Options.ServerName);
            }
            Steps.Add(new Step<string>(new()
            {
                Prompt = serverPrompt,
                ValueReceiver = (v) => Options.ServerName = v,
                SkipChecker = IsAdminTemplate
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Enter the [{Constants.PROMPT_COLOR}]database[/] name:")
                    .AllowEmpty()
                    .DefaultValue(Options.DatabaseName),
                ValueReceiver = (v) => Options.DatabaseName = v,
                SkipChecker = IsAdminTemplate
            }));

            var useExistingPrompt = new ConfirmationPrompt($"Use [{Constants.PROMPT_COLOR}]existing[/] database?")
            {
                DefaultValue = Options.UseExistingDatabase
            };
            Steps.Add(new Step<bool>(new()
            {
                Prompt = useExistingPrompt,
                ValueReceiver = (v) => Options.UseExistingDatabase = v,
                SkipChecker = IsAdminTemplate
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>($"Enter the admin [{Constants.PROMPT_COLOR}]password[/]:")
                    .AllowEmpty()
                    .DefaultValue(Options.AdminPassword),
                ValueReceiver = (v) => Options.AdminPassword = v,
                SkipChecker = IsAdminTemplate
            }));
        }


        private bool IsAdminTemplate() => Options.Template.Equals(Constants.TEMPLATE_ADMIN, StringComparison.OrdinalIgnoreCase);
    }
}
