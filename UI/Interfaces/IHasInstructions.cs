namespace SpacePirates.Console.UI.Interfaces
{
    public interface IHasInstructions
    {
        string[] Instructions { get; }
        (string Key, string Description)[] QuickKeys { get; }
    }
} 