using SpacePirates.API.Models;
using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Core.Models.Movement;
using SpacePirates.Console.UI.Views.Map;
using System;
using System.Collections.Generic;

namespace SpacePirates.Console.UI.Views
{
    public class SolarSystemMapView : MapView, ISelectableMapView
    {
        private readonly SolarSystem _system;
        private Planet? _planetUnderCursor = null;
        private IGameState? _gameState = null;
        private ShipTrail? _shipTrail = null;

        public SolarSystemMapView(SolarSystem system, (int X, int Y, int Width, int Height) bounds)
            : base(bounds)
        {
            _system = system;
        }

        public void SetShipTrail(ShipTrail? trail)
        {
            _shipTrail = trail;
        }

        protected override void RenderMapObjects(IBufferWriter buffer)
        {
            // Center of the map area
            int centerX = _bounds.X + _bounds.Width / 2;
            int centerY = _bounds.Y + _bounds.Height / 2;
            int radius = Math.Min(_bounds.Width, _bounds.Height) / 3;
            var planetPositions = new Dictionary<(int X, int Y), Planet>();
            int planetCount = _system.Planets.Count;
            for (int i = 0; i < planetCount; i++)
            {
                // Spread planets in a circle
                double angle = 2 * Math.PI * i / planetCount;
                int x = centerX + (int)(radius * Math.Cos(angle));
                int y = centerY + (int)(radius * Math.Sin(angle));
                planetPositions[(x, y)] = _system.Planets[i];
            }

            // Draw ship trail if available
            if (_gameState?.PlayerShip != null && _shipTrail != null)
            {
                var trail = _shipTrail.GetTrail();
                int n = trail.Count;
                for (int t = 0; t < n; t++)
                {
                    var (tx, ty) = trail[t];
                    int drawX = _bounds.X + tx;
                    int drawY = _bounds.Y + ty;
                    ConsoleColor color = t switch {
                        int idx when idx < n / 4 => ConsoleColor.DarkGray,
                        int idx when idx < n / 2 => ConsoleColor.Gray,
                        int idx when idx < 3 * n / 4 => ConsoleColor.White,
                        _ => ConsoleColor.Yellow
                    };
                    buffer.DrawChar(drawX, drawY, '·', color);
                }
            }

            // Draw planets and cursor
            _planetUnderCursor = null;
            for (int y = _bounds.Y + 1; y < _bounds.Y + _bounds.Height - 1; y++)
            {
                for (int x = _bounds.X + 1; x < _bounds.X + _bounds.Width - 1; x++)
                {
                    bool isCursor = (x == _cursorX && y == _cursorY);
                    if (planetPositions.TryGetValue((x, y), out var planet))
                    {
                        if (isCursor)
                        {
                            buffer.DrawChar(x, y, '●', ConsoleColor.Black, ConsoleColor.Yellow);
                            _planetUnderCursor = planet;
                        }
                        else
                            buffer.DrawChar(x, y, '●', ConsoleColor.Yellow, ConsoleColor.Black);
                    }
                    else if (isCursor)
                    {
                        buffer.DrawChar(x, y, ' ', null, ConsoleColor.Yellow);
                    }
                    else
                    {
                        buffer.DrawChar(x, y, ' ', null, ConsoleColor.Black);
                    }
                }
            }

            // Draw ship if available
            if (_gameState?.PlayerShip != null)
            {
                var ship = _gameState.PlayerShip;
                int shipX = _bounds.X + (int)ship.Position.X;
                int shipY = _bounds.Y + (int)ship.Position.Y;
                // Draw pulsing shield if active or charging
                if (ship.Shield != null && (ship.Shield.IsActive || ship.Shield.Charging))
                {
                    int pulse = (int)((DateTime.UtcNow.Millisecond / 1000.0) * 10) % 2;
                    ConsoleColor shieldColor = pulse == 0 ? ConsoleColor.Cyan : ConsoleColor.Blue;
                    int radiusX = 2;
                    int radiusY = 1;
                    for (int dx = -radiusX; dx <= radiusX; dx++)
                    {
                        for (int dy = -radiusY; dy <= radiusY; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            double dist = Math.Pow(dx / (double)radiusX, 2) + Math.Pow(dy / (double)radiusY, 2);
                            if (dist >= 0.8 && dist <= 1.4)
                            {
                                int sx = shipX + dx;
                                int sy = shipY + dy;
                                if (sx > _bounds.X && sx < _bounds.X + _bounds.Width - 1 && sy > _bounds.Y && sy < _bounds.Y + _bounds.Height - 1)
                                {
                                    buffer.DrawChar(sx, sy, 'o', shieldColor);
                                }
                            }
                        }
                    }
                }
                char shipChar = ship.Shield?.IsActive == true ? '⊡' : '□';
                ConsoleColor shipColor = ship.Shield?.IsActive == true ? ConsoleColor.Cyan : ConsoleColor.White;
                buffer.DrawChar(shipX, shipY, shipChar, shipColor);
            }
        }

        protected override void RenderDetailsPanel(IBufferWriter buffer)
        {
            int panelX = 0, panelY = 0, panelWidth = 30, panelHeight = 30; // Use config as needed
            buffer.Clear(panelX, panelY, panelWidth, panelHeight);
            buffer.DrawBox(panelX, panelY, panelWidth, panelHeight, BoxStyle.Single);
            int y = panelY + 1;
            if (_planetUnderCursor != null)
            {
                buffer.DrawString(panelX + 2, y++, "PLANET", ConsoleColor.Cyan);
                buffer.DrawString(panelX + 2, y++, $"Name: {_planetUnderCursor.Name}", ConsoleColor.White);
                buffer.DrawString(panelX + 2, y++, $"Type: {_planetUnderCursor.PlanetType}", ConsoleColor.Gray);
                buffer.DrawString(panelX + 2, y++, $"Resources:", ConsoleColor.Yellow);
                foreach (var res in _planetUnderCursor.Resources)
                {
                    buffer.DrawString(panelX + 4, y++, $"{res.Resource.Name}: {res.AmountAvailable}", ConsoleColor.DarkYellow);
                }
            }
            else
            {
                buffer.DrawString(panelX + 2, y++, "SOLAR SYSTEM", ConsoleColor.Cyan);
                buffer.DrawString(panelX + 2, y++, $"Name: {_system.Name}", ConsoleColor.White);
                buffer.DrawString(panelX + 2, y++, $"Sun: {_system.SunType}", ConsoleColor.Yellow);
                buffer.DrawString(panelX + 2, y++, $"Planets:", ConsoleColor.Gray);
                foreach (var planet in _system.Planets)
                {
                    buffer.DrawString(panelX + 4, y++, $"- {planet.Name} ({planet.PlanetType})", ConsoleColor.Gray);
                }
            }
        }

        public override void Update(SpacePirates.Console.Core.Interfaces.IGameState gameState)
        {
            _gameState = gameState;
        }

        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            base.HandleInput(keyInfo);
            if (char.ToLower(keyInfo.KeyChar) == 'd')
            {
                _showDetails = !_showDetails;
            }
        }

        public override void Render()
        {
            // This method is required by BaseView, but actual rendering uses Render(IBufferWriter buffer)
            // You may want to throw NotImplementedException or leave empty if not used directly
        }

        public override string[] Instructions => new[] { "Move: mxy (ie. m1a)" };
        public override (string Key, string Description)[] QuickKeys => new[] {
            ("h/j/k/l", "Move cursor"),
            ("g", "Go back to galaxy map"),
            ("m", "Start a Move command"),
            ("s", "Toggle shield")
        };

        public object? SelectedObject => _planetUnderCursor;
    }
} 