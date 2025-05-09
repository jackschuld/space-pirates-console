using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.Core.Interfaces;
using SpacePirates.API.Models;
using System.Linq;
using SpacePirates.Console.UI.Components;

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

            buffer.DrawString(textX, y++, $"Name: {_system.Name}", ConsoleColor.Cyan);
            string hexId = _system.Name.Contains("-") ? _system.Name[( _system.Name.LastIndexOf('-') + 1 )..] : _system.Name;
            buffer.DrawString(textX, y++, $"ID: {hexId}", ConsoleColor.Yellow);
            buffer.DrawString(textX, y++, $"Sun: {_system.SunType}", ConsoleColor.Yellow);
            buffer.DrawString(textX, y++, $"Planets: {_system.Planets.Count}", PanelStyles.CommandTextColor);
            foreach (var planet in _system.Planets)
            {
                var planetColor = StatusPanelStyle.GetPlanetColor(planet);
                buffer.DrawString(textX + 1, y++, $"{planet.Name.ToUpper()}", planetColor);
                buffer.DrawString(textX + 4, y++, $"({planet.PlanetType})", ConsoleColor.Gray);
                foreach (var res in planet.Resources)
                {
                    var color = StatusPanelStyle.GetResourceColor(res.Resource.Name);
                    var sciName = StatusPanelStyle.GetScientificResourceName(res.Resource.Name);
                    buffer.DrawString(textX + 6, y++, $"{sciName}: {res.AmountAvailable}", color);
                }
            }
        }
    }
} 