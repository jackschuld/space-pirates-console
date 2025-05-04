using SpacePirates.Console.Core.Interfaces;
using SpacePirates.API.Models;

namespace SpacePirates.Console.UI.Components
{
    public class GameViewComponent : IGameComponent
    {
        private readonly (int X, int Y, int Width, int Height) _bounds;
        private IGameState? _gameState;
        // Store the usable area dimensions (excluding borders)
        private readonly int _usableWidth;
        private readonly int _usableHeight;

        public GameViewComponent(int x, int y, int width, int height)
        {
            _bounds = (x, y, width, height);
            // Calculate usable area (subtract 2 for borders)
            _usableWidth = width - 2;
            _usableHeight = height - 2;
        }

        public (int X, int Y, int Width, int Height) Bounds => _bounds;

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
                    if (ship.Position.X < mapSize.X) ship.Position.X += 1;
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

            // Draw game border
            buffer.DrawBox(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, BoxStyle.Double);

            if (_gameState?.PlayerShip == null) return;

            var ship = _gameState.PlayerShip;
            
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