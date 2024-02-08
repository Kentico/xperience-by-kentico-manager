using Xperience.Xman.Configuration;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// Describes an executable command from the CLI.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// If <c>true</c>, execution should stop.
        /// </summary>
        public bool StopProcessing { get; set; }


        /// <summary>
        /// A list of errors encountered during execution.
        /// </summary>
        public List<string> Errors { get; }


        /// <summary>
        /// The parameters in the first position that will trigger this command.
        /// </summary>
        public IEnumerable<string> Keywords { get; }


        /// <summary>
        /// The allowed parameters for single-parameter commands.
        /// </summary>
        public IEnumerable<string> Parameters { get; }


        /// <summary>
        /// A description to display in the Help menu.
        /// </summary>
        public string Description { get; }


        /// <summary>
        /// If <c>true</c>, the command requires a <see cref="ToolProfile"/> to be active and
        /// will throw an exception if not set.
        /// </summary>
        public bool RequiresProfile { get; set; }


        /// <summary>
        /// Runs before <see cref="Execute"/>.
        /// </summary>
        /// <param name="profile">The active profile.</param>
        /// <param name="action">A value from <see cref="Parameters"/> passed, if any</param>
        public Task PreExecute(ToolProfile? profile, string? action);


        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="profile">The active profile.</param>
        /// <param name="action">A value from <see cref="Parameters"/> passed, if any</param>
        public Task Execute(ToolProfile? profile, string? action);


        /// <summary>
        /// Runs after <see cref="Execute"/>.
        /// </summary>
        /// <param name="profile">The active profile.</param>
        /// <param name="action">A value from <see cref="Parameters"/> passed, if any</param>
        public Task PostExecute(ToolProfile? profile, string? action);
    }
}
