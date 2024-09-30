using Xperience.Manager.Options;
using Xperience.Manager.Steps;

namespace Xperience.Manager.Wizards
{
    public interface IWizard<TOptions> where TOptions : IWizardOptions
    {
        /// <summary>
        /// A list of <see cref="Step{T}"/>s to used to populate <see cref="Options"/>.
        /// </summary>
        public StepList Steps { get; }


        /// <summary>
        /// The options to populate.
        /// </summary>
        public TOptions Options { get; set; }


        /// <summary>
        /// Initializes the <see cref="Steps"/> with the <see cref="Step{T}"/>s required to
        /// populate the <see cref="Options"/>.
        /// </summary>
        /// <param name="args">Optional arguments to pass to the step initialization.</param>
        public Task InitSteps(params string[] args);


        /// <summary>
        /// Requests user input to generate the <see cref="Options"/>.
        /// </summary>
        /// <param name="args">Optional arguments to pass to the wizard.</param>
        public Task<TOptions> Run(params string[] args);
    }
}
