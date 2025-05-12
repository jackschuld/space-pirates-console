using System;
using SpacePirates.API.Models;

namespace SpacePirates.Console.UI.Helpers
{
    public static class MapRenderer
    {
        public static (char symbol, ConsoleColor color) GetStarSymbolAndColor(string starType)
        {
            switch (starType.ToUpperInvariant())
            {
                case "G": // Yellow Dwarf (like the Sun)
                    return ('☼', ConsoleColor.Yellow);
                case "K": // Orange Dwarf
                    return ('◉', ConsoleColor.DarkYellow);
                case "M": // Red Dwarf
                    return ('●', ConsoleColor.Red);
                case "B": // Blue Giant
                    return ('◍', ConsoleColor.Cyan);
                case "O": // Blue Supergiant
                    return ('◎', ConsoleColor.Blue);
                case "A": // White Dwarf
                    return ('◌', ConsoleColor.Gray);
                case "F": // Yellow-White
                    return ('◯', ConsoleColor.White);
                default:
                    return ('★', ConsoleColor.White);
            }
        }

        public static string GetStarDescriptor(string starType)
        {
            switch (starType.ToUpperInvariant())
            {
                case "G": return "Yellow Dwarf";
                case "K": return "Orange Dwarf";
                case "M": return "Red Dwarf";
                case "B": return "Blue Giant";
                case "O": return "Blue Supergiant";
                case "A": return "White Dwarf";
                case "F": return "Yellow-White";
                default: return "Unknown Star";
            }
        }

        public static char GetPlanetIcon(Planet planet)
        {
            if (planet == null || !planet.IsDiscovered) return '?';
            if (planet.PlanetType?.ToLowerInvariant() == "gas giant")
                return '●'; // Large icon for gas giants
            return '•'; // Small icon for terrestrial
        }
    }
} 