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
            // Clear help area
            buffer.Clear(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height);

            // No border for command area

            // Center the command/help text
            if (!string.IsNullOrEmpty(_helpText))
            {
                int textX = _bounds.X + (_bounds.Width - _helpText.Length) / 2;
                int textY = _bounds.Y + _bounds.Height / 2;
                buffer.DrawString(textX, textY, _helpText, ConsoleColor.DarkGreen);
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