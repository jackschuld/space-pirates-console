namespace SpacePirates.Console.UI.Views.Game.Panel.Instructions
{
    public static class InstructionsData
    {
        public static readonly string[] Commands = new[]
        {
            "Move: x y or xy",
        };
        public static readonly (string Key, string Description)[] QuickKeys = new[]
        {
            ("m", "Start a Move command"),
            ("Tab", "Toggle this panel (Status/Instructions)"),
            ("ESC", "Quit game")
        };
        public const string Header = "INSTRUCTIONS";
    }
}