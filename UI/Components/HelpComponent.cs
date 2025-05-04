using SpacePirates.Console.Core.Interfaces;
using SpacePirates.API.Models;

namespace SpacePirates.Console.UI.Components
{
    public class HelpComponent : IHelpComponent
    {
        private readonly (int X, int Y, int Width, int Height) _bounds;
        private string _helpText = "Use arrow keys to navigate | SPACE to select | ESC to exit";

        public HelpComponent(int x, int y, int width, int height)
        {
            _bounds = (x, y, width, height);
        }

        public (int X, int Y, int Width, int Height) Bounds => _bounds;

        public void Render(IBufferWriter buffer)
        {
            // Clear help area
            buffer.Clear(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height);

            // Draw help border
            buffer.DrawBox(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, BoxStyle.Rounded);

            // Center the help text
            if (!string.IsNullOrEmpty(_helpText))
            {
                int textX = _bounds.X + (_bounds.Width - _helpText.Length) / 2;
                int textY = _bounds.Y + _bounds.Height / 2;
                buffer.DrawString(textX, textY, _helpText, ConsoleColor.Gray);
            }
        }

        public void Update(IGameState gameState)
        {
            // Help component doesn't need game state updates
        }

        public void SetHelpText(string text)
        {
            _helpText = text;
        }
    }
} 