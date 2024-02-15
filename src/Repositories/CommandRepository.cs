using Xperience.Manager.Commands;

namespace Xperience.Manager.Repositories
{
    /// <summary>
    /// Contains all <see cref="ICommand"/>s registered at startup.
    /// </summary>
    public class CommandRepository : ICommandRepository
    {
        private readonly IEnumerable<ICommand> commands;


        public CommandRepository(IEnumerable<ICommand> commands) => this.commands = commands;


        public ICommand? Get(string keyword) => commands.FirstOrDefault(c => c?.Keywords.Contains(keyword, StringComparer.OrdinalIgnoreCase) ?? false);


        public IEnumerable<ICommand> GetAll() => commands;
    }
}
