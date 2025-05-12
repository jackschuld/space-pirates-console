using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.Core.Interfaces;
using SpacePirates.API.Models;
using System.Linq;
using SpacePirates.Console.UI.Components;
using SpacePirates.Console.UI.Helpers;

namespace SpacePirates.Console.UI.Views
{
    public class SolarSystemStatusView : StatusView
    {
        private SolarSystem? _system;

        public SolarSystemStatusView(BaseControls controls, StatusPanelStyle styleProvider, (int X, int Y, int Width, int Height) bounds, SolarSystem? system)
            : base(controls, styleProvider, bounds, "SOLAR SYSTEM STATUS")
        {
            _system = system;
        }

        protected override void RenderDetails(IBufferWriter buffer, int textX, ref int y)
        {
            if (_system == null)
            {
                buffer.DrawString(textX, y++, "No system selected.", PanelStyles.CommandTextColor);
                return;
            }
            string hexId = _system.Name.Contains("-") ? _system.Name[( _system.Name.LastIndexOf('-') + 1 )..] : _system.Name;
            buffer.DrawString(textX, y++, $"ID: {hexId}", ConsoleColor.Yellow);
            if (_system.Star == null || !_system.Star.IsDiscovered)
            {
                buffer.DrawBox(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, BorderStyle);
                buffer.DrawString(textX, y++, $"Name: ???", ConsoleColor.DarkGray);
                buffer.DrawString(textX, y++, $"Sun: ???", ConsoleColor.DarkGray);
                buffer.DrawString(textX, y++, $"Planets: ???", ConsoleColor.DarkGray);
                buffer.DrawString(textX, y + 4, "Warp using 'w' to reveal star details.", ConsoleColor.DarkGray);
                buffer.DrawString(textX, y + 5, "system details.", ConsoleColor.DarkGray);
                return;
            }
            buffer.DrawString(textX, y++, $"Name: {_system.Name}", ConsoleColor.Cyan);
            var starDescriptor = MapRenderer.GetStarDescriptor(_system.Star.Type);
            buffer.DrawString(textX, y++, $"Sun: {starDescriptor}", ConsoleColor.Yellow);
            buffer.DrawString(textX, y++, $"Planets: {_system.Planets.Count}", PanelStyles.CommandTextColor);
            foreach (var planet in _system.Planets)
            {
                if (!planet.IsDiscovered)
                {
                    buffer.DrawString(textX + 1, y++, "???", ConsoleColor.DarkGray);
                    buffer.DrawString(textX + 4, y++, "(Unknown)", ConsoleColor.DarkGray);
                    y++; // Space for resources
                }
                else
                {
                    var planetColor = StatusPanelStyle.GetPlanetColor(planet);
                    buffer.DrawString(textX + 1, y++, $"{planet.Name.ToUpper()}", planetColor);
                    buffer.DrawString(textX + 4, y++, $"({planet.PlanetType})", ConsoleColor.Gray);
                    foreach (var res in planet.Resources)
                    {
                        var color = ResourceHelper.GetResourceColor(res.Resource.Name);
                        var sciName = ResourceHelper.GetResourceName(res.Resource.Name);
                        buffer.DrawString(textX + 6, y++, $"{sciName}: {res.AmountAvailable}", color);
                    }
                }
            }
        }

        public void SetSystem(SolarSystem? system)
        {
            _system = system;
        }

        public override void Update(IGameState gameState)
        {
            if (_system == null) return;
            // Cast to concrete GameState to access Galaxy
            if (gameState is SpacePirates.Console.Core.Models.State.GameState concreteState && concreteState.Galaxy != null)
            {
                var updatedSystem = concreteState.Galaxy.SolarSystems.FirstOrDefault(s => s.Id == _system.Id);
                if (updatedSystem != null)
                    _system = updatedSystem;
            }
        }
    }
} 