﻿using Xperience.Manager.Options;
using Xperience.Manager.Steps;

namespace Xperience.Manager.Wizards
{
    /// <summary>
    /// A configuration wizard which displays steps for user input and populates an <see cref="IWizardOptions"/>
    /// with the provided data.
    /// </summary>
    public abstract class AbstractWizard<TOptions> : IWizard<TOptions> where TOptions : IWizardOptions, new()
    {
        public StepList Steps { get; } = [];


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
