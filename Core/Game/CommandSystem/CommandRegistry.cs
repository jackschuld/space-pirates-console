using System.Collections.Generic;

namespace SpacePirates.Console.Game.CommandSystem
{
    public class CommandRegistry
    {
        private readonly Dictionary<string, ICommand> _commands = new();

        public void Register(ICommand command) => _commands[command.Name] = command;
        public ICommand? GetCommand(string name) => _commands.TryGetValue(name, out var cmd) ? cmd : null;
        public IEnumerable<ICommand> GetAllCommands() => _commands.Values;
    }
} 