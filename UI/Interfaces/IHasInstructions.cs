namespace SpacePirates.Console.UI.Views
{
    public interface IHasInstructions
    {
        string[] Instructions { get; }
        (string Key, string Description)[] QuickKeys { get; }
    }
} 