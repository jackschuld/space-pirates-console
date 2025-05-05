using SpacePirates.Console.Core.Interfaces;

namespace SpacePirates.Console.UI.ConsoleRenderer
{
    public class ConsoleBufferWriter : IBufferWriter
    {
        private readonly ConsoleBuffer[,] _buffer;
        private readonly int _width;
        private readonly int _height;
        private readonly (int X, int Y) _offset;

        private static readonly Dictionary<BoxStyle, (char TopLeft, char TopRight, char BottomLeft, char BottomRight, char Horizontal, char Vertical)> BoxChars = new()
        {
            [BoxStyle.Single] = ('┌', '┐', '└', '┘', '─', '│'),
            [BoxStyle.Double] = ('╔', '╗', '╚', '╝', '═', '║'),
            [BoxStyle.Rounded] = ('╭', '╮', '╰', '╯', '─', '│')
        };

        public ConsoleBufferWriter(ConsoleBuffer[,] buffer, int width, int height, (int X, int Y) offset)
        {
            _buffer = buffer;
            _width = width;
            _height = height;
            _offset = offset;
        }

        public void DrawChar(int x, int y, char character, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            int bufferX = x + _offset.X;
            int bufferY = y + _offset.Y;

            if (!IsInBounds(bufferX, bufferY)) return;

            _buffer[bufferX, bufferY].Character = character;
            if (foreground.HasValue) _buffer[bufferX, bufferY].Foreground = foreground.Value;
            if (background.HasValue) _buffer[bufferX, bufferY].Background = background.Value;
            _buffer[bufferX, bufferY].IsDirty = true;
        }

        public void DrawString(int x, int y, string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (x + i >= _width) break;
                DrawChar(x + i, y, text[i], foreground, background);
            }
        }

        public void DrawBox(int x, int y, int width, int height, BoxStyle style = BoxStyle.Single, ConsoleColor? borderColor = null)
        {
            var chars = BoxChars[style];

            // Draw corners
            DrawChar(x, y, chars.TopLeft, borderColor);
            DrawChar(x + width - 1, y, chars.TopRight, borderColor);
            DrawChar(x, y + height - 1, chars.BottomLeft, borderColor);
            DrawChar(x + width - 1, y + height - 1, chars.BottomRight, borderColor);

            // Draw horizontal lines
            for (int i = 1; i < width - 1; i++)
            {
                DrawChar(x + i, y, chars.Horizontal, borderColor);
                DrawChar(x + i, y + height - 1, chars.Horizontal, borderColor);
            }

            // Draw vertical lines
            for (int i = 1; i < height - 1; i++)
            {
                DrawChar(x, y + i, chars.Vertical, borderColor);
                DrawChar(x + width - 1, y + i, chars.Vertical, borderColor);
            }
        }

        public void Clear(int x, int y, int width, int height)
        {
            for (int dy = 0; dy < height; dy++)
            {
                for (int dx = 0; dx < width; dx++)
                {
                    DrawChar(x + dx, y + dy, ' ');
                }
            }
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        // Overload to satisfy IBufferWriter interface
        public void DrawBox(int x, int y, int width, int height, BoxStyle style)
        {
            DrawBox(x, y, width, height, style, null);
        }
    }
} 