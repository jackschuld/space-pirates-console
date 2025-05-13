using SpacePirates.API.Models;
using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Core.Models.State;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpacePirates.Console.UI.Views.Map
{
    public class GalaxyMapView : MapView, ISelectableMapView
    {
        private Galaxy _galaxy;
        private SolarSystem? _selectedSystem = null;
        private SolarSystem? _systemUnderCursor = null;
        public SolarSystem? SystemUnderCursor => _systemUnderCursor;
        public object? SelectedObject => _systemUnderCursor;

        public GalaxyMapView(Galaxy galaxy, (int X, int Y, int Width, int Height) bounds)
            : base(bounds)
        {
            _galaxy = galaxy;
        }

        protected override void RenderMapObjects(IBufferWriter buffer)
        {
            SpacePirates.Console.UI.Helpers.MapRenderer.RenderSolarSystems(
                buffer,
                _galaxy.SolarSystems,
                _bounds,
                CursorPosition,
                out _selectedSystem,
                out _systemUnderCursor
            );
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
                int panelX = 0, panelY = 0, panelWidth = 30, panelHeight = 30;
                buffer.Clear(panelX, panelY, panelWidth, panelHeight);
                buffer.DrawBox(panelX, panelY, panelWidth, panelHeight, BoxStyle.Single);
                int y = panelY + 1;
                buffer.DrawString(panelX + 2, y++, $"Galaxy: {_galaxy.Name}", ConsoleColor.Cyan);
                buffer.DrawString(panelX + 2, y++, $"Systems: {_galaxy.SolarSystems.Count}", ConsoleColor.White);
            }
        }

        private void RenderSystemDetails(IBufferWriter buffer, SolarSystem system)
        {
            int panelX = 0, panelY = 0, panelWidth = 30, panelHeight = 30; 
            buffer.Clear(panelX, panelY, panelWidth, panelHeight);
            buffer.DrawBox(panelX, panelY, panelWidth, panelHeight, BoxStyle.Single);
            int y = panelY + 1;
            buffer.DrawString(panelX + 2, y++, $"System: {system.Name}", ConsoleColor.Cyan);
            buffer.DrawString(panelX + 2, y++, $"Sun: {system.SunType}", ConsoleColor.Yellow);
            buffer.DrawString(panelX + 2, y++, $"Planets:", ConsoleColor.White);
            foreach (var planet in system.Planets)
            {
                buffer.DrawString(panelX + 4, y++, $"{planet.Name} ({planet.PlanetType})", ConsoleColor.Gray);
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