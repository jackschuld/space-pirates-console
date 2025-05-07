using SpacePirates.API.Models;
using SpacePirates.Console.Core.Interfaces;
using System;

namespace SpacePirates.Console.UI.Components
{
    public class SolarSystemComponent : IGameComponent
    {
        private readonly SolarSystem _system;
        private readonly (int X, int Y, int Width, int Height) _bounds;

        public SolarSystemComponent(SolarSystem system, (int X, int Y, int Width, int Height) bounds)
        {
            _system = system;
            _bounds = bounds;
        }

        public (int X, int Y, int Width, int Height) Bounds => _bounds;

        public void Render(IBufferWriter buffer)
        {
            buffer.Clear(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height);
            buffer.DrawBox(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, BoxStyle.Double);

            int y = _bounds.Y + 2;
            int x = _bounds.X + 2;
            buffer.DrawString(x, y++, $"Solar System: {_system.Name} (Sun: {_system.SunType})", ConsoleColor.Cyan);

            foreach (var planet in _system.Planets)
            {
                buffer.DrawString(x, y++, $"- {planet.Name} ({planet.PlanetType})", ConsoleColor.Gray);
                foreach (var res in planet.Resources)
                {
                    buffer.DrawString(x + 2, y++, $"  {res.Resource.Name}: {res.AmountAvailable}", ConsoleColor.DarkYellow);
                }
            }
        }

        public void Update(IGameState gameState) { }

        public void HandleInput(ConsoleKeyInfo keyInfo)
        {
            // Add navigation or selection logic as needed
        }
    }
} 