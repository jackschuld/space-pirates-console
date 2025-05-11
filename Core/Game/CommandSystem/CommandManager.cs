using System;
using System.Collections.Generic;

namespace SpacePirates.Console.Game.CommandSystem
{
    public class CommandManager
    {
        private readonly CommandRegistry _registry = new CommandRegistry();
        private CommandParser? _parser;
        private CommandContext? _context;

        public CommandManager(CommandContext context, IEnumerable<ICommand> commands)
        {
            _context = context;
            foreach (var cmd in commands)
                _registry.Register(cmd);
            _parser = new CommandParser(context, _registry.GetAllCommands());
        }

        public void RegisterCommand(ICommand command)
        {
            _registry.Register(command);
            if (_parser != null)
                _parser = new CommandParser(_context!, _registry.GetAllCommands());
        }

        public void SetContext(CommandContext context)
        {
            _context = context;
            if (_parser != null)
                _parser = new CommandParser(context, _registry.GetAllCommands());
        }

        public string Execute(string input)
        {
            if (_parser == null) throw new InvalidOperationException("Command parser not initialized.");
            return _parser.ParseAndExecuteWithResult(input);
        }
    }
} 