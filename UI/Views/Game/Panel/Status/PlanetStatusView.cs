using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.Core.Interfaces;
using SpacePirates.API.Models;
using System;
using System.Linq;
using SpacePirates.Console.UI.Components;
using SpacePirates.Console.UI.Helpers;
namespace SpacePirates.Console.UI.Views
{
    public class PlanetStatusView : StatusView
    {
        private Planet? _planet;
        private SolarSystem? _system;

        public PlanetStatusView(BaseControls controls, StatusPanelStyle styleProvider, (int X, int Y, int Width, int Height) bounds, Planet? planet, SolarSystem? system)
            : base(controls, styleProvider, bounds, "PLANET STATUS")
        {
            _planet = planet;
            _system = system;
        }

        protected override void RenderDetails(IBufferWriter buffer, int textX, ref int y)
        {
            if (buffer == null) return;
            if (_planet == null)
            {
                buffer.DrawString(textX, y++, "No planet selected.", PanelStyles.CommandTextColor);
                return;
            }
            buffer.DrawString(textX, y++, $"ID: {(_planet.Name.Contains("-") ? _planet.Name[( _planet.Name.LastIndexOf('-') + 1 )..] : _planet.Name)}", ConsoleColor.Yellow);
            int planetIndex = _system?.Planets.IndexOf(_planet) ?? -1;
            int planetCount = _system?.Planets.Count ?? 0;
            if (!_planet.IsDiscovered)
            {
                buffer.DrawBox(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, BorderStyle);
                buffer.DrawString(textX, y++, $"Coordinates: (?, ?)", ConsoleColor.Gray);
                var undiscoveredColor = PlanetColors.GetPlanetColor(_planet);
                buffer.DrawString(textX, y++, $"Name: ???", undiscoveredColor);
                buffer.DrawString(textX, y++, $"Type: ???", ConsoleColor.DarkGray);
                buffer.DrawString(textX, y++, $"Resources: ???", ConsoleColor.DarkGray);
                buffer.DrawString(textX, y + 4, "Examine planet with 'e' to reveal", ConsoleColor.DarkGray);
                buffer.DrawString(textX, y + 5, "details.", ConsoleColor.DarkGray);
                return;
            }
            if (planetIndex >= 0 && planetCount > 0)
            {
                // Brute-force: build the map's planetPositions and find the (x, y) for this planet
                int minX = _bounds.X + 1;
                int maxX = _bounds.X + _bounds.Width - 2;
                int minY = _bounds.Y + 1;
                int maxY = _bounds.Y + _bounds.Height - 2;
                double planetRadiusX = (maxX - minX) / 2.2;
                double planetRadiusY = (maxY - minY) / 2.2;
                int mapCenterX = minX + (maxX - minX) / 2;
                int mapCenterY = minY + (maxY - minY) / 2;
                int systemSeed = _system?.Id ?? 0;
                var rand = new Random(systemSeed);
                (int x, int y)? found = null;
                for (int i = 0; i < planetCount; i++)
                {
                    double baseAngle = 2 * Math.PI * i / planetCount;
                    double angleOffset = (rand.NextDouble() - 0.5) * (Math.PI / planetCount);
                    double angle = baseAngle + angleOffset;
                    double radiusOffsetX = planetRadiusX * (0.9 + 0.2 * rand.NextDouble());
                    double radiusOffsetY = planetRadiusY * (0.9 + 0.2 * rand.NextDouble());
                    int x = mapCenterX + (int)(radiusOffsetX * Math.Cos(angle));
                    int mapY = mapCenterY + (int)(radiusOffsetY * Math.Sin(angle));
                    x = Math.Max(minX, Math.Min(maxX, x));
                    x += (int)(x*.9) - 1;
                    mapY = Math.Max(minY, Math.Min(maxY, mapY));
                    var candidate = _system.Planets[i];
                    if (object.ReferenceEquals(candidate, _planet) || candidate.Id == _planet.Id)
                    {
                        found = (x, mapY);
                        break;
                    }
                }
                if (found.HasValue)
                {
                    buffer.DrawString(textX, y++, $"Coordinates: ({found.Value.x}, {StatusView.YCoordToLetter(found.Value.y)})", ConsoleColor.Gray);
                }
            }
            var planetColor = PlanetColors.GetPlanetColor(_planet);
            buffer.DrawString(textX, y++, $"Name: {_planet.Name}", planetColor);
            buffer.DrawString(textX, y++, $"Type: {_planet.PlanetType}", ConsoleColor.Gray);
            buffer.DrawString(textX, y++, $"Resources:", ConsoleColor.Yellow);
            foreach (var res in _planet.Resources)
            {
                var color = ResourceHelper.GetResourceColor(res.Resource.Name);
                var sciName = ResourceHelper.GetResourceName(res.Resource.Name);
                buffer.DrawString(textX + 2, y++, $"{sciName}: {res.AmountAvailable}", color);
            }
        }
    }
} 