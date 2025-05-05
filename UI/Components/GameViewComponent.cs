using SpacePirates.Console.Core.Interfaces;
using SpacePirates.API.Models;
using SpacePirates.Console.Core.Models.State;
using SpacePirates.Console.UI.Components; // For PanelStyles
using SpacePirates.Console.Core.Models.Movement;

namespace SpacePirates.Console.UI.Components
{
    public class GameViewComponent : IGameComponent
    {
        private readonly (int X, int Y, int Width, int Height) _bounds;
        private IGameState? _gameState;
        // Store the usable area dimensions (excluding borders)
        private readonly int _usableWidth;
        private readonly int _usableHeight;
        private ShipTrail? _shipTrail;
        public ShipTrail? ShipTrail { get => _shipTrail; set => _shipTrail = value; }

        public GameViewComponent(int x, int y, int width, int height)
        {
            _bounds = (x, y, width, height);
            // Calculate usable area (subtract 2 for borders)
            _usableWidth = width - 2;
            _usableHeight = height - 2;
        }

        public (int X, int Y, int Width, int Height) Bounds => _bounds;

        // Expose usable area and left border for modular rendering
        public int UsableWidth => _usableWidth;
        public int UsableHeight => _usableHeight;
        public int LeftBorderX => _bounds.X + 1; // First usable X after left border

        public void HandleInput(ConsoleKeyInfo keyInfo)
        {
            if (_gameState?.PlayerShip == null) return;

            var ship = _gameState.PlayerShip;
            var mapSize = _gameState.MapSize;
            
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    if (ship.Position.Y > 1) ship.Position.Y -= 1;
                    break;
                case ConsoleKey.DownArrow:
                    if (ship.Position.Y < mapSize.Y) ship.Position.Y += 1;
                    break;
                case ConsoleKey.LeftArrow:
                    if (ship.Position.X > 1) ship.Position.X -= 1;
                    break;
                case ConsoleKey.RightArrow:
                    if (ship.Position.X < 75) ship.Position.X += 1;
                    break;
                case ConsoleKey.Spacebar:
                    if (ship.Shield != null)
                    {
                        ship.Shield.IsActive = !ship.Shield.IsActive;
                    }
                    break;
            }
        }

        public void Render(IBufferWriter buffer)
        {
            // Clear the game area
            buffer.Clear(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height);

            // Draw game border in dark yellow if possible
            if (buffer is SpacePirates.Console.UI.ConsoleRenderer.ConsoleBufferWriter cbw)
                cbw.DrawBox(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, BoxStyle.Double, PanelStyles.BorderColor);
            else
                buffer.DrawBox(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, BoxStyle.Double);

            // Draw selected capital letters vertically along the right side of the game area
            int[] letterIndices = { 0, 5, 10, 15, 20, 25 }; // A, F, K, P, U, Z
            int lettersX = _bounds.X + _bounds.Width; // Just outside the right border
            int lettersStartY = _bounds.Y + 1; // Start just below the top border
            int letterAreaHeight = _usableHeight;
            for (int i = 0; i < letterIndices.Length; i++)
            {
                int letterIndex = letterIndices[i];
                char letter = (char)('A' + letterIndex);
                // Evenly space the letters from top to bottom
                int y = lettersStartY + (int)Math.Round(i * (letterAreaHeight - 1) / (double)(letterIndices.Length - 1));
                buffer.DrawString(lettersX, y, " " + letter.ToString(), ConsoleColor.White);
            }

            if (_gameState?.PlayerShip == null) return;

            var ship = _gameState.PlayerShip;

            // Draw ship trail (fading)
            if (_shipTrail != null)
            {
                var trail = _shipTrail.GetTrail();
                int n = trail.Count;
                for (int i = 0; i < n; i++)
                {
                    var (tx, ty) = trail[i];
                    int drawX = _bounds.X + tx;
                    int drawY = _bounds.Y + ty;
                    // Fade: oldest is darkest, newest is lightest
                    ConsoleColor color = i switch {
                        int idx when idx < n / 4 => ConsoleColor.DarkGray,
                        int idx when idx < n / 2 => ConsoleColor.Gray,
                        int idx when idx < 3 * n / 4 => ConsoleColor.White,
                        _ => ConsoleColor.Yellow
                    };
                    buffer.DrawChar(drawX, drawY, '·', color);
                }
            }

            // Calculate ship position (add border offset)
            int shipX = _bounds.X + (int)ship.Position.X;
            int shipY = _bounds.Y + (int)ship.Position.Y;

            // Draw ship with shield status
            char shipChar = ship.Shield?.IsActive == true ? '⊡' : '□';
            ConsoleColor shipColor = ship.Shield?.IsActive == true ? ConsoleColor.Cyan : ConsoleColor.White;
            buffer.DrawChar(shipX, shipY, shipChar, shipColor);
        }

        public void Update(IGameState gameState)
        {
            _gameState = gameState;
        }
    }
} 