﻿using Xperience.Xman.Options;
using Xperience.Xman.Steps;

namespace Xperience.Xman.Wizards
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
        public Task InitSteps();


        /// <summary>
        /// Requests user input to generate the <see cref="Options"/>.
        /// </summary>
        public Task<TOptions> Run();
    }
}
