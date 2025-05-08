using SpacePirates.API.Models;
using SpacePirates.Console.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace SpacePirates.Console.UI.Views
{
    public class SolarSystemMapView : MapView
    {
        private readonly SolarSystem _system;
        private Planet? _planetUnderCursor = null;

        public SolarSystemMapView(SolarSystem system, (int X, int Y, int Width, int Height) bounds)
            : base(bounds)
        {
            _system = system;
        }

        protected override void RenderMapObjects(IBufferWriter buffer)
        {
            int offsetX = _bounds.X + 6, offsetY = _bounds.Y + 4;
            var planetPositions = new Dictionary<(int X, int Y), Planet>();
            int i = 0;
            foreach (var planet in _system.Planets)
            {
                int x = offsetX;
                int y = offsetY + i * 5;
                planetPositions[(x, y)] = planet;
                i++;
            }

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

        public override void Update(SpacePirates.Console.Core.Interfaces.IGameState gameState) { }

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
            ("d", "Show details (toggle)"),
            ("g", "Go back to galaxy map"),
            ("m", "Start a Move command"),
            ("s", "Toggle shield")
        };
    }
} 