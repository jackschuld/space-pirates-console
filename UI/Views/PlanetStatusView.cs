using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.Core.Interfaces;
using SpacePirates.API.Models;
using System;
using System.Linq;
using SpacePirates.Console.UI.Components;

namespace SpacePirates.Console.UI.Views
{
    public class PlanetStatusView : StatusView
    {
        private Planet? _planet;

        public PlanetStatusView(BaseControls controls, StatusPanelStyle styleProvider, (int X, int Y, int Width, int Height) bounds, Planet? planet)
            : base(controls, styleProvider, bounds, "PLANET STATUS")
        {
            _planet = planet;
        }

        protected override void RenderDetails(IBufferWriter buffer, int textX, ref int y)
        {
            if (_planet == null)
            {
                buffer.DrawString(textX, y++, "No planet selected.", PanelStyles.CommandTextColor);
                return;
            }

            buffer.DrawString(textX, y++, $"Name: {_planet.Name}", ConsoleColor.Cyan);
            buffer.DrawString(textX, y++, $"Type: {_planet.PlanetType}", ConsoleColor.Gray);
            buffer.DrawString(textX, y++, $"Resources:", ConsoleColor.Yellow);
            foreach (var res in _planet.Resources)
            {
                var color = StatusPanelStyle.GetResourceColor(res.Resource.Name);
                var sciName = StatusPanelStyle.GetScientificResourceName(res.Resource.Name);
                buffer.DrawString(textX + 2, y++, $"{sciName}: {res.AmountAvailable}", color);
            }
        }
    }
} 