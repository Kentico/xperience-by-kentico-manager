using Spectre.Console;

using Xperience.Manager.Helpers;
using Xperience.Manager.Options;
using Xperience.Manager.Steps;

namespace Xperience.Manager.Wizards
{
    /// <summary>
    /// A wizard which generates an <see cref="UpdateOptions"/> for updating Xperience by Kentico.
    /// </summary>
    public class UpdateWizard : AbstractWizard<UpdateOptions>
    {
        public override async Task InitSteps(params string[] args)
        {
            var versions = await NuGetVersionHelper.GetPackageVersions(Constants.TEMPLATES_PACKAGE);
            var filtered = versions.Where(v => !v.IsPrerelease && !v.IsLegacyVersion && v.Major >= Constants.MIN_LISTED_VERSION)
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
        }
    }
}
