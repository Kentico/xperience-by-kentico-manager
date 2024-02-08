using Spectre.Console;

namespace Xperience.Xman.Steps
{
    /// <summary>
    /// The configuration of a <see cref="Step{T}"/>.
    /// </summary>
    public class StepOptions<T>
    {
        /// <summary>
        /// The prompt to show the user when the step begins.
        /// </summary>
        public IPrompt<T>? Prompt { get; set; }


        /// <summary>
        /// A function that is passed the user input.
        /// </summary>
        public Action<T>? ValueReceiver { get; set; }


        /// <summary>
        /// A function that returns <c>true</c> if this step should be skipped.
        /// </summary>
        public Func<bool>? SkipChecker { get; set; }
    }
}
