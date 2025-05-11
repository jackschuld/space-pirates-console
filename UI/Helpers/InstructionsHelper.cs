namespace SpacePirates.Console.UI.Views.Game.Panel.Instructions
{
    public static class InstructionsHelper
    {
        public static (string Key, string Description)[] GetDefaultQuickKeys() => new[]
        {
            ("Tab", "Toggle this panel (Status/Instructions)"),
            ("ESC", "Quit game")
        };
    }
}