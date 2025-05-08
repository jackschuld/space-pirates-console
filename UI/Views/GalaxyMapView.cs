using SpacePirates.API.Models;
using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Core.Models.State;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpacePirates.Console.UI.Views
{
    public class GalaxyMapView : MapView
    {
        private Galaxy _galaxy;
        private SolarSystem? _selectedSystem = null;
        private SolarSystem? _systemUnderCursor = null;
        public SolarSystem? SystemUnderCursor => _systemUnderCursor;

        public GalaxyMapView(Galaxy galaxy, (int X, int Y, int Width, int Height) bounds)
            : base(bounds)
        {
            _galaxy = galaxy;
        }

        protected override void RenderMapObjects(IBufferWriter buffer)
        {
            int offsetX = _bounds.X + 5, offsetY = _bounds.Y + 2;
            var systemPositions = new Dictionary<(int X, int Y), SolarSystem>();
            foreach (var sys in _galaxy.SolarSystems)
            {
                int x = offsetX + (int)(sys.X / 2);
                int y = offsetY + (int)(sys.Y / 4);
                var key = (x, y);
                if (!systemPositions.ContainsKey(key))
                    systemPositions[key] = sys;
            }
            _selectedSystem = null;
            _systemUnderCursor = null;
            for (int y = _bounds.Y + 1; y < _bounds.Y + _bounds.Height - 1; y++)
            {
                for (int x = _bounds.X + 1; x < _bounds.X + _bounds.Width - 1; x++)
                {
                    bool isCursor = (x == _cursorX && y == _cursorY);
                    if (systemPositions.TryGetValue((x, y), out var sys))
                    {
                        if (isCursor)
                        {
                            buffer.DrawChar(x, y, 'O', ConsoleColor.Black, ConsoleColor.Yellow);
                            _selectedSystem = sys;
                            _systemUnderCursor = sys;
                        }
                        else
                            buffer.DrawChar(x, y, 'O', ConsoleColor.Yellow, ConsoleColor.Black);
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
            buffer.DrawString(_bounds.X + 2, _bounds.Y, $"Galaxy: {_galaxy.Name}", ConsoleColor.Cyan, ConsoleColor.Black);
        }

        protected override void RenderDetailsPanel(IBufferWriter buffer)
        {
            if (_showDetails && _selectedSystem != null)
            {
                RenderSystemDetails(buffer, _selectedSystem);
            }
            else
            {
                int panelX = 0, panelY = 0, panelWidth = 30, panelHeight = 30; // Use config as needed
                buffer.Clear(panelX, panelY, panelWidth, panelHeight);
                buffer.DrawBox(panelX, panelY, panelWidth, panelHeight, BoxStyle.Single);
                int y = panelY + 1;
                buffer.DrawString(panelX + 2, y++, $"Galaxy: {_galaxy.Name}", ConsoleColor.Cyan);
                buffer.DrawString(panelX + 2, y++, $"Systems: {_galaxy.SolarSystems.Count}", ConsoleColor.White);
            }
        }

        private void RenderSystemDetails(IBufferWriter buffer, SolarSystem system)
        {
            int panelX = 0, panelY = 0, panelWidth = 30, panelHeight = 30; // Use config as needed
            buffer.Clear(panelX, panelY, panelWidth, panelHeight);
            buffer.DrawBox(panelX, panelY, panelWidth, panelHeight, BoxStyle.Single);
            int y = panelY + 1;
            buffer.DrawString(panelX + 2, y++, $"System: {system.Name}", ConsoleColor.Cyan);
            buffer.DrawString(panelX + 2, y++, $"Sun: {system.SunType}", ConsoleColor.Yellow);
            buffer.DrawString(panelX + 2, y++, $"Planets:", ConsoleColor.White);
            foreach (var planet in system.Planets)
            {
                buffer.DrawString(panelX + 4, y++, $"{planet.Name.ToUpper()} ({planet.PlanetType})", ConsoleColor.Gray);
                foreach (var res in planet.Resources)
                {
                    buffer.DrawString(panelX + 6, y++, $"{res.Resource.Name}: {res.AmountAvailable}", ConsoleColor.DarkYellow);
                }
            }
        }

        public override void Update(IGameState gameState) { }

        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            base.HandleInput(keyInfo);
            if (char.ToLower(keyInfo.KeyChar) == 'd')
            {
                _showDetails = !_showDetails;
            }
        }

        public override string[] Instructions => new[] { "Warp: w + System ID" };
        public override (string Key, string Description)[] QuickKeys => new[] {
            ("h/j/k/l", "Move cursor"),
            ("w", "Start a Warp command")
        };

        public override void Render()
        {
            // This method is required by BaseView, but actual rendering uses Render(IBufferWriter buffer)
            // You may want to throw NotImplementedException or leave empty if not used directly
        }
    }
} 