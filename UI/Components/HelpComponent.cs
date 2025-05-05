using SpacePirates.Console.Core.Interfaces;
using SpacePirates.API.Models;

namespace SpacePirates.Console.UI.Components
{
    public class CommandComponent : IHelpComponent
    {
        private readonly (int X, int Y, int Width, int Height) _bounds;
        private string _helpText = "Type 'c' to enter command mode | ESC to exit";

        public CommandComponent(int x, int y, int width, int height)
        {
            _bounds = (x, y, width, height);
        }

        public (int X, int Y, int Width, int Height) Bounds => _bounds;

        public void Render(IBufferWriter buffer)
        {
            // Draw background for command area (now default black)
            for (int y = _bounds.Y; y < _bounds.Y + _bounds.Height; y++)
            {
                for (int x = _bounds.X; x < _bounds.X + _bounds.Width; x++)
                {
                    buffer.DrawChar(x, y, ' ', ConsoleColor.DarkGreen, ConsoleColor.Black);
                }
            }

            // Optionally, draw a border (uncomment if you want a border)
            // buffer.DrawBox(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, BoxStyle.Single, ConsoleColor.Blue);

            // Center the command/help text
            if (!string.IsNullOrEmpty(_helpText))
            {
                int textX = _bounds.X + (_bounds.Width - _helpText.Length) / 2;
                int textY = _bounds.Y + _bounds.Height / 2;
                buffer.DrawString(textX, textY, _helpText, ConsoleColor.Green, ConsoleColor.Black);
            }
        }

        public void Update(IGameState gameState)
        {
            // Command component doesn't need game state updates
        }

        public void SetHelpText(string text)
        {
            _helpText = text;
        }
    }
} 