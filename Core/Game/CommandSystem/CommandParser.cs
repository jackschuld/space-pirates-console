using System;
using System.Linq;
using System.Collections.Generic;

namespace SpacePirates.Console.Game.CommandSystem
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
            var trimmed = input.Trim();
            // Extract command name (letters only at start)
            int i = 0;
            while (i < trimmed.Length && (char.IsLetter(trimmed[i]) || trimmed[i] == 'c')) i++;
            var commandName = trimmed.Substring(0, i).TrimStart('c').ToLower();
            var rest = trimmed.Substring(i).Trim();
            var args = new List<string>();
            if (!string.IsNullOrEmpty(rest))
            {
                // If there are spaces, split as usual
                if (rest.Contains(' '))
                {
                    args.AddRange(rest.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    // No spaces: try to extract numbers and letters
                    var current = "";
                    foreach (char c in rest)
                    {
                        if (char.IsDigit(c))
                        {
                            if (current.Length > 0 && !char.IsDigit(current[^1]))
                            {
                                args.Add(current);
                                current = "";
                            }
                            current += c;
                        }
                        else if (char.IsLetter(c))
                        {
                            if (current.Length > 0 && !char.IsLetter(current[^1]))
                            {
                                args.Add(current);
                                current = "";
                            }
                            current += c;
                        }
                        else
                        {
                            if (current.Length > 0)
                            {
                                args.Add(current);
                                current = "";
                            }
                        }
                    }
                    if (current.Length > 0)
                        args.Add(current);
                }
            }

            var command = _commands.Values.FirstOrDefault(cmd =>
                string.Equals(cmd.Name, commandName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(cmd.ShortName, commandName, StringComparison.OrdinalIgnoreCase));

            if (command != null)
            {
                _context.Result = null;
                command.Execute(_context, args.ToArray());
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