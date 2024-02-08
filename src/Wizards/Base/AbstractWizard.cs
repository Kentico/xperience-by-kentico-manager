using Xperience.Xman.Options;
using Xperience.Xman.Steps;

namespace Xperience.Xman.Wizards
{
    /// <summary>
    /// A configuration wizard which displays steps for user input and populates an <see cref="IWizardOptions"/>
    /// with the provided data.
    /// </summary>
    public abstract class AbstractWizard<TOptions> : IWizard<TOptions> where TOptions : IWizardOptions, new()
    {
        public StepList Steps { get; } = new();


        public TOptions Options { get; set; } = new();


        public abstract Task InitSteps();


        public async Task<TOptions> Run()
        {
            await InitSteps();
            do
            {
                await Steps.Current.Execute();
            } while (Steps.Next());

            return Options;
        }
    }
}
