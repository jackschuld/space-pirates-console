namespace SpacePirates.Console.UI.InputHandling.CommandSystem
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        void Execute(CommandContext context, string[] args);
    }
} 