using System;
using SpacePirates.API.Models;
using SpacePirates.Console.UI.Helpers;

namespace SpacePirates.Console.UI.Styles
{
    public class StatusPanelStyle : BaseStyle
    {
        public static ConsoleColor GetPlanetColor(Planet planet)
        {
            if (planet.Resources != null && planet.Resources.Count > 0)
            {
                var dominant = planet.Resources.OrderByDescending(r => r.AmountAvailable).First();
                return ResourceHelper.GetResourceColor(dominant.Resource.Name);
            }
            var type = planet.PlanetType.ToLowerInvariant();
            if (type.Contains("gas")) return ConsoleColor.DarkCyan;
            return ConsoleColor.White;
        }
    }
} 