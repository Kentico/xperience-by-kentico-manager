using Xperience.Manager.Commands;

namespace Xperience.Manager.Repositories
{
    /// <summary>
    /// Represents a collection of <see cref="ICommand"/>s.
    /// </summary>
    public interface ICommandRepository : IRepository
    {
        /// <summary>
        /// Gets a command from the repository where <see cref="ICommand.Keywords"/> contains the provided string,
        /// or <c>null</c> if not found.
        /// </summary>
        public ICommand? Get(string keyword);


        /// <summary>
        /// Gets all commands in the repository.
        /// </summary>
        public IEnumerable<ICommand> GetAll();
    }
}