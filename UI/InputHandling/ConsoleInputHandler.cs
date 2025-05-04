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
            return HasPendingCommands ? _commandQueue.Dequeue() : new Command(Core.Interfaces.CommandType.None);
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
                ConsoleKey.W or ConsoleKey.UpArrow => new Command(Core.Interfaces.CommandType.Move, Direction.Up),
                ConsoleKey.S or ConsoleKey.DownArrow => new Command(Core.Interfaces.CommandType.Move, Direction.Down),
                ConsoleKey.A or ConsoleKey.LeftArrow => new Command(Core.Interfaces.CommandType.Move, Direction.Left),
                ConsoleKey.D or ConsoleKey.RightArrow => new Command(Core.Interfaces.CommandType.Move, Direction.Right),
                ConsoleKey.Spacebar => new Command(Core.Interfaces.CommandType.ToggleShields),
                ConsoleKey.C => new Command(Core.Interfaces.CommandType.ViewCargo),
                ConsoleKey.Escape => new Command(Core.Interfaces.CommandType.Exit),
                _ => null
            };
        }
    }

    internal class Command : ICommand
    {
        public Core.Interfaces.CommandType Type { get; }
        public object? Data { get; }

        public Command(Core.Interfaces.CommandType type, object? data = null)
        {
            Type = type;
            Data = data;
        }
    }
} 