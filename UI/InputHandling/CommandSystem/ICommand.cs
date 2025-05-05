namespace SpacePirates.Console.UI.InputHandling.CommandSystem
{
    public interface ICommand
    {
        string Name { get; }      // Full command name, e.g., "move"
        string ShortName { get; } // Short command alias, e.g., "m"
        string Description { get; }
        void Execute(CommandContext context, string[] args);
    }
} 