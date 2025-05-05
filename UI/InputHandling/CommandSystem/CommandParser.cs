using System;
using System.Linq;
using System.Collections.Generic;

namespace SpacePirates.Console.UI.InputHandling.CommandSystem
{
    public class CommandParser
    {
        private readonly Dictionary<string, ICommand> _commands;
        private readonly CommandContext _context;

        public CommandParser(CommandContext context, IEnumerable<ICommand> commands)
        {
            _context = context;
            _commands = commands.ToDictionary(cmd => cmd.Name, StringComparer.OrdinalIgnoreCase);
        }

        public void ParseAndExecute(string input)
        {
            ParseAndExecuteWithResult(input);
        }

        public string ParseAndExecuteWithResult(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var commandName = parts[0].TrimStart(':');
            var args = parts.Skip(1).ToArray();

            if (_commands.TryGetValue(commandName, out var command))
            {
                _context.Result = null;
                command.Execute(_context, args);
                return _context.Result ?? string.Empty;
            }
            else
            {
                var error = $"'{commandName}' is not a command";
                _context.Result = error;
                return error;
            }
        }
    }
} 