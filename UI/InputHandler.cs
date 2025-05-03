namespace SpacePirates.Console.UI;

public enum CommandType
{
    None,
    Move,
    Attack,
    ToggleShields,
    ViewCargo,
    Exit
}

public record Command(CommandType Type, object? Data = null);

public class InputHandler
{
    public Command GetCommand()
    {
        var key = System.Console.ReadKey(true);
        
        return key.Key switch
        {
            ConsoleKey.UpArrow => new Command(CommandType.Move, Direction.Up),
            ConsoleKey.DownArrow => new Command(CommandType.Move, Direction.Down),
            ConsoleKey.LeftArrow => new Command(CommandType.Move, Direction.Left),
            ConsoleKey.RightArrow => new Command(CommandType.Move, Direction.Right),
            ConsoleKey.F => new Command(CommandType.Attack),
            ConsoleKey.S => new Command(CommandType.ToggleShields),
            ConsoleKey.C => new Command(CommandType.ViewCargo),
            ConsoleKey.Q => new Command(CommandType.Exit),
            _ => new Command(CommandType.None)
        };
    }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
} 