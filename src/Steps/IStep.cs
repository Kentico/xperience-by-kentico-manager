namespace Xperience.Xman.Steps
{
    /// <summary>
    /// Represents a step used to display a prompt for user interaction.
    /// </summary>
    public interface IStep
    {
        /// <summary>
        /// Displays a prompt to the user.
        /// </summary>
        public Task Execute();
    }
}
