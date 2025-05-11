namespace SpacePirates.Console.UI.ConsoleRenderer
{
    public class ConsoleBuffer
    {
        public char Character { get; set; }
        public ConsoleColor Foreground { get; set; }
        public ConsoleColor Background { get; set; }
        public bool IsDirty { get; set; }

        public ConsoleBuffer()
        {
            Character = ' ';
            Foreground = ConsoleColor.Gray;
            Background = ConsoleColor.Black;
            IsDirty = true;
        }
    }
} 