using SpacePirates.API.Models;
using SpacePirates.Console.Core.Interfaces;
using System;
using SpacePirates.Console;
using SpacePirates.Console.Core.Models.State;
using System.Linq;
using System.Collections.Generic;

namespace SpacePirates.Console.UI.Components
{
    public class GalaxyMapComponent : IGameComponent
    {
        private Galaxy _galaxy;
        private readonly (int X, int Y, int Width, int Height) _bounds;
        private int _cursorX;
        private int _cursorY;

        public GalaxyMapComponent(Galaxy galaxy)
            : this(galaxy, (ConsoleConfig.StatusAreaWidth, 0, ConsoleConfig.GAME_AREA_WIDTH, ConsoleConfig.MainAreaHeight))
        {
            // Start cursor in the center of the map area
            _cursorX = _bounds.X + _bounds.Width / 2;
            _cursorY = _bounds.Y + _bounds.Height / 2;
        }

        public GalaxyMapComponent(Galaxy galaxy, (int X, int Y, int Width, int Height) bounds)
        {
            _galaxy = galaxy;
            _bounds = bounds;
            _cursorX = _bounds.X + _bounds.Width / 2;
            _cursorY = _bounds.Y + _bounds.Height / 2;
        }

        public (int X, int Y, int Width, int Height) Bounds => _bounds;

        public void Render(IBufferWriter buffer)
        {
            // Draw border in dark yellow if possible
            if (buffer is SpacePirates.Console.UI.ConsoleRenderer.ConsoleBufferWriter cbw)
                cbw.DrawBox(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, BoxStyle.Double, PanelStyles.BorderColor);
            else
                buffer.DrawBox(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, BoxStyle.Double);

            int offsetX = _bounds.X + 5, offsetY = _bounds.Y + 2;
            // Build a dictionary of all solar system positions
            var systemPositions = new Dictionary<(int X, int Y), SolarSystem>();
            foreach (var sys in _galaxy.SolarSystems)
            {
                int x = offsetX + (int)(sys.X / 2);
                int y = offsetY + (int)(sys.Y / 4);
                var key = (x, y);
                if (!systemPositions.ContainsKey(key))
                    systemPositions[key] = sys;
                // else: skip or handle collision as you wish
            }
            // Build a dictionary of all name letter positions
            var namePositions = new Dictionary<(int X, int Y), char>();
            foreach (var sys in _galaxy.SolarSystems)
            {
                int x = offsetX + (int)(sys.X / 2);
                int y = offsetY + (int)(sys.Y / 4);
                for (int i = 0; i < sys.Name.Length; i++)
                {
                    namePositions[(x + 2 + i, y)] = sys.Name[i];
                }
            }
            // Redraw the map area cell by cell
            for (int y = _bounds.Y + 1; y < _bounds.Y + _bounds.Height - 1; y++)
            {
                for (int x = _bounds.X + 1; x < _bounds.X + _bounds.Width - 1; x++)
                {
                    if (systemPositions.TryGetValue((x, y), out var sys))
                    {
                        buffer.DrawChar(x, y, 'O', ConsoleColor.Yellow, ConsoleColor.Black);
                    }
                    else if (namePositions.TryGetValue((x, y), out var c))
                    {
                        buffer.DrawChar(x, y, c, ConsoleColor.Gray, ConsoleColor.Black);
                    }
                    else
                    {
                        buffer.DrawChar(x, y, ' ', null, ConsoleColor.Black);
                    }
                }
            }
            buffer.DrawString(_bounds.X + 2, _bounds.Y, $"Galaxy: {_galaxy.Name}", ConsoleColor.Cyan, ConsoleColor.Black);
        }

        public void Update(IGameState gameState)
        {
            // Handle input for selecting a solar system, etc.
        }

        public void HandleInput(ConsoleKeyInfo keyInfo)
        {
            // Only handle galaxy map controls
            switch (char.ToLower(keyInfo.KeyChar))
            {
                case 'h':
                    _cursorX = Math.Max(_bounds.X + 1, _cursorX - 1);
                    break;
                case 'l':
                    _cursorX = Math.Min(_bounds.X + _bounds.Width - 2, _cursorX + 1);
                    break;
                case 'k':
                    _cursorY = Math.Max(_bounds.Y + 1, _cursorY - 1);
                    break;
                case 'j':
                    _cursorY = Math.Min(_bounds.Y + _bounds.Height - 2, _cursorY + 1);
                    break;
            }
        }

        public string? GetSelectedSystemId()
        {
            int offsetX = _bounds.X + 5, offsetY = _bounds.Y + 2;
            var systemPositions = _galaxy.SolarSystems.ToDictionary(
                sys => (X: offsetX + (int)(sys.X / 2), Y: offsetY + (int)(sys.Y / 4)),
                sys => sys
            );
            if (systemPositions.TryGetValue((_cursorX, _cursorY), out var sys))
                return sys.Id.ToString();
            return null;
        }
    }
} 