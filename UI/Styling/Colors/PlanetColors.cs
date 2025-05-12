using SpacePirates.API.Models;
using System;
using System.Linq;

namespace SpacePirates.Console.UI.Helpers
{
    public static class PlanetColors
    {
        public static ConsoleColor GetPlanetColor(Planet planet)
        {
            if (planet == null || planet.Resources == null || planet.Resources.Count == 0)
            {
                return planet.PlanetType?.ToLowerInvariant() == "gas giant" ? ConsoleColor.Blue : ConsoleColor.Gray;
            }
            var maxResource = planet.Resources
                .OrderByDescending(r => r.AmountAvailable)
                .FirstOrDefault();
            if (maxResource != null)
            {
                return ResourceHelper.GetResourceColor(maxResource.Resource.Name);
            }
            // Fallback by type
            return planet.PlanetType?.ToLowerInvariant() == "gas giant" ? ConsoleColor.Blue : ConsoleColor.Gray;
        }
    }
} 