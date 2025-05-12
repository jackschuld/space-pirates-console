using System;
using SpacePirates.API.Models;
using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Core.Models.Movement;
using SpacePirates.Console.UI.Views.Map;
using SpacePirates.Console.UI.Helpers;
using System.Collections.Generic;
using System.Linq;

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
            int radius = Math.Min(_bounds.Width, _bounds.Height) / 2 - 2;

            // Use the star's coordinates as the center
            double starX = _system.Star?.X ?? _system.X;
            double starY = _system.Star?.Y ?? _system.Y;

            // Place planets in a circle within the visible map grid (1-75, 1-30)
            int planetCount = _system.Planets.Count;
            int minX = _bounds.X + 1;
            int maxX = _bounds.X + _bounds.Width - 2;
            int minY = _bounds.Y + 1;
            int maxY = _bounds.Y + _bounds.Height - 2;
            double planetRadiusX = (maxX - minX) / 2.2;
            double planetRadiusY = (maxY - minY) / 2.2;
            int mapCenterX = minX + (maxX - minX) / 2;
            int mapCenterY = minY + (maxY - minY) / 2;
            var planetPositions = new Dictionary<(int X, int Y), Planet>();
            // Use a deterministic seed based on the solar system's ID to keep planet positions fixed
            int systemSeed = _system.Id;
            var rand = new Random(systemSeed);
            for (int i = 0; i < planetCount; i++)
            {
                // Add random offset to angle and radius for more natural, staggered orbits
                double baseAngle = 2 * Math.PI * i / planetCount;
                double angleOffset = (rand.NextDouble() - 0.5) * (Math.PI / planetCount); // up to ±half the segment
                double angle = baseAngle + angleOffset;
                double radiusOffsetX = planetRadiusX * (0.9 + 0.2 * rand.NextDouble()); // 90% to 110% of base radius
                double radiusOffsetY = planetRadiusY * (0.9 + 0.2 * rand.NextDouble());
                int x = mapCenterX + (int)(radiusOffsetX * Math.Cos(angle));
                int y = mapCenterY + (int)(radiusOffsetY * Math.Sin(angle));
                x = Math.Max(minX, Math.Min(maxX, x));
                y = Math.Max(minY, Math.Min(maxY, y));
                planetPositions[(x, y)] = _system.Planets[i];
                _system.Planets[i].X = x;
                _system.Planets[i].Y = y;
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
                        var planetColor = PlanetColors.GetPlanetColor(planet);
                        char planetIcon = MapRenderer.GetPlanetIcon(planet);
                        if (isCursor)
                        {
                            buffer.DrawChar(x, y, planetIcon, ConsoleColor.Black, planetColor);
                            _planetUnderCursor = planet;
                        }
                        else
                        {
                            buffer.DrawChar(x, y, planetIcon, planetColor, ConsoleColor.Black);
                        }
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

            // Draw the star at the center with a symbol and color based on its type
            if (_system.Star != null)
            {
                var (starChar, starColor) = MapRenderer.GetStarSymbolAndColor(_system.Star.Type);
                buffer.DrawChar(mapCenterX, mapCenterY, starChar, starColor);
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
            // Check if ship is near a planet and update command line help text
            if (ParentGameView != null && ParentGameView.CommandLine is CommandLineView clv)
            {
                var nearbyPlanet = GetNearbyPlanet();
                if (nearbyPlanet != null && nearbyPlanet.IsDiscovered)
                {
                    clv.SetHelpText($"Press 'd' to drill planet {nearbyPlanet.Name}");
                }
                else
                {
                    clv.SetHelpText("Tab to toggle instructions | ESC to exit");
                }
            }
        }

        private Planet? GetNearbyPlanet()
        {
            if (_gameState?.PlayerShip == null) return null;
            int shipX = _bounds.X + (int)_gameState.PlayerShip.Position.X;
            int shipY = _bounds.Y + (int)_gameState.PlayerShip.Position.Y;
            const int DRILL_RADIUS = 5;
            return _system.Planets
                .FirstOrDefault(p => Math.Sqrt(Math.Pow((int)Math.Round(p.X) - shipX, 2) + Math.Pow((int)Math.Round(p.Y) - shipY, 2)) <= DRILL_RADIUS);
        }

        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            base.HandleInput(keyInfo);
        }

        public override void Render()
        {
            // This method is required by BaseView, but actual rendering uses Render(IBufferWriter buffer)
            // You may want to throw NotImplementedException or leave empty if not used directly
        }

        public override string[] Instructions => new[] { 
            "Fly: fxy (ie. f1a)",
            "Examine: exx00 (ie. ex4d2)"
             };
        public override (string Key, string Description)[] QuickKeys => new[] {
            ("h/j/k/l", "Move cursor"),
            ("e", "Examine planet (reveal details)"),
            ("f", "Start a Fly command"),
            ("d", "Drill planet (mine resources)"),
            ("s", "Toggle shield"),
            ("r", "Go back to galaxy map")
        };

        public object? SelectedObject => _planetUnderCursor;

        public Planet? GetPlanetUnderCursor()
        {
            int mapCenterX = _bounds.X + _bounds.Width / 2;
            int mapCenterY = _bounds.Y + _bounds.Height / 2;
            int radius = Math.Min(_bounds.Width, _bounds.Height) / 3;
            var planetPositions = new Dictionary<(int X, int Y), Planet>();
            int planetCount = _system.Planets.Count;
            for (int i = 0; i < planetCount; i++)
            {
                double angle = 2 * Math.PI * i / planetCount;
                int x = mapCenterX + (int)(radius * Math.Cos(angle));
                int y = mapCenterY + (int)(radius * Math.Sin(angle));
                planetPositions[(x, y)] = _system.Planets[i];
            }
            planetPositions.TryGetValue((_cursorX, _cursorY), out var planet);
            return planet;
        }

        public SolarSystem System => _system;

        public static (int x, int y) GetPlanetMapCoordinates(int planetIndex, int planetCount, (int X, int Y, int Width, int Height) bounds)
        {
            int minX = bounds.X + 1;
            int maxX = bounds.X + bounds.Width - 2;
            int minY = bounds.Y + 1;
            int maxY = bounds.Y + bounds.Height - 2;
            double planetRadiusX = (maxX - minX) / 2.2;
            double planetRadiusY = (maxY - minY) / 2.2;
            int mapCenterX = minX + (maxX - minX) / 2;
            int mapCenterY = minY + (maxY - minY) / 2;
            // Use deterministic random for this planet
            int systemSeed = bounds.GetHashCode() ^ planetCount; // fallback if no system ID
            var rand = new Random(systemSeed);
            for (int i = 0; i < planetIndex; i++) // advance RNG to correct state
            {
                rand.NextDouble(); rand.NextDouble(); rand.NextDouble(); rand.NextDouble();
            }
            double baseAngle = 2 * Math.PI * planetIndex / planetCount;
            double angleOffset = (rand.NextDouble() - 0.5) * (Math.PI / planetCount);
            double angle = baseAngle + angleOffset;
            double radiusOffsetX = planetRadiusX * (0.9 + 0.2 * rand.NextDouble());
            double radiusOffsetY = planetRadiusY * (0.9 + 0.2 * rand.NextDouble());
            int x = mapCenterX + (int)(radiusOffsetX * Math.Cos(angle));
            int y = mapCenterY + (int)(radiusOffsetY * Math.Sin(angle));
            x = Math.Max(minX, Math.Min(maxX, x));
            y = Math.Max(minY, Math.Min(maxY, y));
            return (x, y);
        }

        public GameView? ParentGameView { get; set; }
    }

    public static class SolarSystemExtensions
    {
        public static void UpdateFrom(this SolarSystem target, SolarSystem source)
        {
            target.Name = source.Name;
            target.X = source.X;
            target.Y = source.Y;
            target.SunType = source.SunType;
            // Update star
            if (target.Star != null && source.Star != null)
            {
                target.Star.Name = source.Star.Name;
                target.Star.X = source.Star.X;
                target.Star.Y = source.Star.Y;
                target.Star.Type = source.Star.Type;
                if (target.Star.GetType().GetProperty("IsDiscovered") != null && source.Star.GetType().GetProperty("IsDiscovered") != null)
                    target.Star.GetType().GetProperty("IsDiscovered")?.SetValue(target.Star, source.Star.GetType().GetProperty("IsDiscovered")?.GetValue(source.Star));
            }
            // Update planets
            target.Planets.Clear();
            foreach (var planet in source.Planets)
            {
                target.Planets.Add(planet);
            }
        }
    }
} 