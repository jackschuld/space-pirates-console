using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Core.Models.Galaxy;
using System;

namespace SpacePirates.Console.UI.Components
{
    public class GalaxyMapComponent : IGameComponent
    {
        private readonly (int X, int Y, int Width, int Height) _bounds;
        private Galaxy _galaxy;
        private GalaxyMapState _state;
        private Action<string>? _onInspect;
        private Action<string>? _onLightspeedPrompt;

        public GalaxyMapComponent(int x, int y, int width, int height, Galaxy galaxy, GalaxyMapState state, Action<string>? onInspect = null, Action<string>? onLightspeedPrompt = null)
        {
            _bounds = (x, y, width, height);
            _galaxy = galaxy;
            _state = state;
            _onInspect = onInspect;
            _onLightspeedPrompt = onLightspeedPrompt;
        }

        public (int X, int Y, int Width, int Height) Bounds => _bounds;

        public void Render(IBufferWriter buffer)
        {
            // Clear the map area (excluding border)
            buffer.Clear(_bounds.X + 1, _bounds.Y + 1, _bounds.Width - 2, _bounds.Height - 2);
            // Draw border
            buffer.DrawBox(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, BoxStyle.Double);
            // Draw faint stars as background
            var rand = new Random(_galaxy.Seed);
            for (int i = 0; i < 120; i++)
            {
                int sx = _bounds.X + 2 + rand.Next(Galaxy.Width - 4);
                int sy = _bounds.Y + 1 + rand.Next(Galaxy.Height - 2);
                buffer.DrawChar(sx, sy, '.', ConsoleColor.DarkGray);
            }

            // Draw all solar systems (not selected)
            foreach (var system in _galaxy.Systems)
            {
                // If this system is at the cursor, skip for now (we'll draw it with the cursor below)
                if (_state.CursorX == system.MapX && _state.CursorY == system.MapY)
                    continue;
                var art = system.GetAsciiArt();
                for (int row = 0; row < art.Length; row++)
                {
                    int drawY = _bounds.Y + system.MapY + row;
                    int drawX = _bounds.X + system.MapX;
                    buffer.DrawString(drawX, drawY, art[row], ConsoleColor.White, ConsoleColor.Black);
                }
            }

            // Clamp cursor so it never covers the border
            int minX = _bounds.X + 1;
            int minY = _bounds.Y + 1;
            int maxX = _bounds.X + _bounds.Width - 2;
            int maxY = _bounds.Y + _bounds.Height - 2;
            int cursorX = Math.Clamp(_bounds.X + _state.CursorX, minX, maxX);
            int cursorY = Math.Clamp(_bounds.Y + _state.CursorY, minY, maxY);

            // Draw cursor: if over a solar system, draw its art in yellow with cyan background; else draw cyan block
            var selectedSystem = _galaxy.GetSystemAt(_state.CursorX, _state.CursorY, 0); // exact match only
            if (selectedSystem != null)
            {
                var art = selectedSystem.GetAsciiArt();
                // Only draw the first char of the art (since cursor is 1x1)
                buffer.DrawString(cursorX, cursorY, art[0], ConsoleColor.Yellow, ConsoleColor.Cyan);
            }
            else
            {
                buffer.DrawChar(cursorX, cursorY, '█', ConsoleColor.Cyan);
            }
        }

        public void HandleInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.H:
                    _state.CursorX = Math.Max(2, _state.CursorX - 1);
                    break;
                case ConsoleKey.L:
                    if ((keyInfo.Modifiers & ConsoleModifiers.Shift) != 0)
                    {
                        var sys = _galaxy.GetSystemAt(_state.CursorX, _state.CursorY);
                        if (sys != null)
                            _onLightspeedPrompt?.Invoke("Are you sure you want to lightspeed to this system? (y/n)");
                    }
                    else
                    {
                        _state.CursorX = Math.Min(Galaxy.Width - 3, _state.CursorX + 1);
                    }
                    break;
                case ConsoleKey.J:
                    _state.CursorY = Math.Min(Galaxy.Height - 2, _state.CursorY + 1);
                    break;
                case ConsoleKey.K:
                    _state.CursorY = Math.Max(1, _state.CursorY - 1);
                    break;
                case ConsoleKey.I:
                    var system = _galaxy.GetSystemAt(_state.CursorX, _state.CursorY);
                    if (system != null)
                        _onInspect?.Invoke($"System: {system.Name}  (X:{system.MapX}, Y:{system.MapY})");
                    break;
                case ConsoleKey.Tab:
                    System.Console.Write((char)9); // Let engine handle Tab for instructions
                    break;
                case ConsoleKey.Escape:
                    System.Console.Write((char)27); // Let engine handle Esc for exit
                    break;
            }
        }

        public void Update(IGameState gameState) { }
    }
} 