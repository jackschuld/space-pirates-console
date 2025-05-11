namespace SpacePirates.Console.Core.Interfaces
{
    public interface IInputHandler
    {
        ICommand GetCommand();
        void ProcessInput();
        bool HasPendingCommands { get; }
    }

    public interface ICommand
    {
        CommandType Type { get; }
        object? Data { get; }
    }

    public enum CommandType
    {
        None,
        Move,
        Attack,
        ToggleShields,
        ViewCargo,
        Exit
    }
} 