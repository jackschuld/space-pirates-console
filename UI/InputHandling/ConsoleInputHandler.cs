using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Core.Models.Movement;

namespace SpacePirates.Console.UI.InputHandling
{
    public class ConsoleInputHandler : IInputHandler
    {
        private Queue<ICommand> _commandQueue = new();

        public bool HasPendingCommands => _commandQueue.Count > 0;

        public ICommand GetCommand()
        {
            return HasPendingCommands ? _commandQueue.Dequeue() : new Command(CommandType.None);
        }

        public void ProcessInput()
        {
            if (!System.Console.KeyAvailable) return;

            var key = System.Console.ReadKey(true);
            var command = MapKeyToCommand(key);
            
            if (command != null)
            {
                _commandQueue.Enqueue(command);
            }
        }

        private ICommand? MapKeyToCommand(ConsoleKeyInfo key)
        {
            return key.Key switch
            {
                ConsoleKey.W or ConsoleKey.UpArrow => new Command(CommandType.Move, Direction.Up),
                ConsoleKey.S or ConsoleKey.DownArrow => new Command(CommandType.Move, Direction.Down),
                ConsoleKey.A or ConsoleKey.LeftArrow => new Command(CommandType.Move, Direction.Left),
                ConsoleKey.D or ConsoleKey.RightArrow => new Command(CommandType.Move, Direction.Right),
                ConsoleKey.Spacebar => new Command(CommandType.ToggleShields),
                ConsoleKey.C => new Command(CommandType.ViewCargo),
                ConsoleKey.Escape => new Command(CommandType.Exit),
                _ => null
            };
        }
    }

    public class Command : ICommand
    {
        public CommandType Type { get; }
        public object? Data { get; }

        public Command(CommandType type, object? data = null)
        {
            Type = type;
            Data = data;
        }
    }
} 