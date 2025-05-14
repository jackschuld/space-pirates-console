using SpacePirates.API.Models;
using SpacePirates.Console.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static void RenderSolarSystems(IBufferWriter buffer, IEnumerable<SolarSystem> systems, (int X, int Y, int Width, int Height) bounds, (int cursorX, int cursorY) cursor, out SolarSystem? selectedSystem, out SolarSystem? systemUnderCursor)
        {
            int offsetX = bounds.X + 5, offsetY = bounds.Y + 2;
            var systemPositions = new Dictionary<(int X, int Y), SolarSystem>();
            foreach (var sys in systems)
            {
                int x = offsetX + (int)(sys.X / 2);
                int y = offsetY + (int)(sys.Y / 4);
                var key = (x, y);
                if (!systemPositions.ContainsKey(key))
                    systemPositions[key] = sys;
            }

            // Procedural starfield background
            int minX = bounds.X + 1;
            int maxX = bounds.X + bounds.Width - 2;
            int minY = bounds.Y + 1;
            int maxY = bounds.Y + bounds.Height - 2;
            ConsoleColor[] starColors = new[] { ConsoleColor.DarkGray, ConsoleColor.Gray, ConsoleColor.White, ConsoleColor.Cyan, ConsoleColor.Blue, ConsoleColor.Magenta };
            char[] starChars = new[] { '.', '.', '.', '.', '.', '.', '.', '.', '*', '·', '·', '·', '+', '·' };
            int bgSeed = bounds.GetHashCode() ^ 0xBEEF;
            // Precompute a background map for this view
            var bgMap = new (char ch, ConsoleColor color)[maxX - minX + 1, maxY - minY + 1];
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int bx = x - minX, by = y - minY;
                    int cellSeed = bgSeed ^ (x * 73856093) ^ (y * 19349663);
                    var cellRand = new Random(cellSeed);
                    double starChance = cellRand.NextDouble();
                    if (starChance < 0.06)
                    {
                        char ch = starChars[cellRand.Next(starChars.Length)];
                        ConsoleColor color;
                        if (ch == '*' || ch == '+' || ch == '·')
                        {
                            color = (cellRand.Next(2) == 0) ? ConsoleColor.White : ConsoleColor.Gray;
                        }
                        else
                        {
                            color = starColors[cellRand.Next(starColors.Length)];
                        }
                        bgMap[bx, by] = (ch, color);
                    }
                    else
                    {
                        bgMap[bx, by] = (' ', ConsoleColor.Black);
                    }
                }
            }

            // Fixed ASCII art template for the galaxy
            string[] galaxyArt = new[] {
                "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⠀⠀⠈⠉⠉⠛⠛⠷⢶⣤⣄⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣀⣤⣴⣶⣶⣿⣿⣿⣿⣷⣶⣦⣤⣄⡀⠈⠙⠻⣷⣦⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⠀⠀⠀⠀⠀⠀⠀⠀⣀⣴⣾⣿⣿⠿⠿⠛⠛⠛⠛⠻⠿⠿⣿⣿⣿⣿⣿⣷⣤⡀⠀⠙⢿⣷⣄⠀⠀⠀⠀⠀⠀⠀⠀",
                "⠀⠀⠀⠀⠀⢀⣴⣾⠿⠛⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠙⠻⣿⣿⣿⣿⣷⡄⠀⠙⢿⣷⡀⠀⠀⠀⠀⠀⠀",
                "⠀⠀⠀⢀⣴⡿⠛⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⢿⣿⣿⣿⣦⠀⠈⢿⣿⡄⠀⠀⠀⠀⠀",
                "⠀⠀⣠⡾⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠒⠒⠒⠤⣄⣀⠀⠀⠀⠻⣿⣿⣿⣷⡀⠀⢿⣿⡀⠀⠀⠀⠀",
                "⠀⣰⠟⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣤⣴⣶⣶⣶⣶⣶⣦⣤⡀⠉⠳⣦⡀⠀⠘⣿⣿⣿⣧⠀⠈⣿⣇⠀⠀⠀⠀",
                "⢰⠏⠀⠀⠀⠀⠀⠀⢀⠔⠀⣠⣴⣿⣿⡿⠛⠋⠉⠉⠉⠉⠙⠻⣿⣷⡀⠈⢷⡀⠀⠸⣿⣿⣿⡄⠀⢹⣿⠀⠀⠀⠀",
                "⠏⠀⠀⠀⠀⠀⢀⡴⠃⢀⣼⣿⣿⠟⠁⠀⠀⡀⠀⢀⣠⣤⡤⣤⡈⢻⣿⡀⠘⣇⠀⠀⣿⣿⣿⡇⠀⢸⣿⠀⠀⠀⠀",
                "⠀⠀⠀⠀⠀⢠⡞⠀⢠⣿⣿⡿⠁⠀⢀⡴⠊⣠⡾⠋⠉⠀⠀⠀⡟⢈⣿⡇⢠⡟⠀⠀⣿⣿⣿⠃⠀⣸⡏⠀⠀⠀⠀",
                "⠀⠀⠀⠀⢠⡟⠀⢀⣿⣿⡿⠀⠀⢠⡏⢠⣾⠏⢀⡠⢲⡖⢀⡜⠁⣼⡿⠀⣼⠃⠀⢰⣿⣿⡿⠀⢀⡿⠁⠀⠀⠀⠀",
                "⠀⠀⠀⠀⣿⠃⠀⣼⣿⣿⡇⠀⢠⡿⠀⣾⡏⢀⡎⠀⠈⠉⠁⢀⣼⠟⢁⡼⠁⠀⢀⣾⣿⡿⠁⠀⡾⠁⠀⠀⠀⠀⠀",
                "⠀⠀⠀⢸⣿⠀⠀⣿⣿⣿⡇⠀⢸⡇⠀⣿⣧⠸⣧⣤⣤⣴⠾⠛⠁⠐⠁⠀⠀⣠⣾⣿⡟⠁⢀⡞⠁⠀⠀⠀⠀⠀⡄",
                "⠀⠀⠀⢸⣿⠀⠀⣿⣿⣿⡇⠀⠈⣷⡀⠹⣿⣦⣀⠀⠀⠀⠀⠀⠀⠀⣀⣤⣾⣿⡿⠋⢀⡴⠋⠀⠀⠀⠀⠀⠀⣰⠃",
                "⠀⠀⠀⢸⣿⡄⠀⢹⣿⣿⣿⡀⠀⠘⢷⡄⠈⠻⢿⣿⣶⣶⣶⣶⣾⣿⣿⡿⠟⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣼⠃⠀",
                "⠀⠀⠀⠈⣿⣧⠀⠈⢿⣿⣿⣷⡄⠀⠀⠙⠳⠤⣀⣈⠉⠉⠙⠋⠉⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⣾⠃⠀⠀",
                "⠀⠀⠀⠀⠘⣿⣧⠀⠈⢿⣿⣿⣿⣦⡀⠀⠀⠀⠀⠀⠈⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⣾⡟⠁⠀⠀⠀",
                "⠀⠀⠀⠀⠀⠙⣿⣧⡀⠀⠻⣿⣿⣿⣿⣦⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣤⣾⡿⠋⠀⠀⠀⠀⠀",
                "⠀⠀⠀⠀⠀⠀⠈⠻⣿⣆⠀⠈⠻⢿⣿⣿⣿⣿⣶⣤⣄⣀⡀⠀⠀⠀⠀⢀⣀⣀⣤⣶⣾⡿⠟⠉⠀⠀⠀⠀⠀⠀⠀",
                "⠀⠀⠀⠀⠀⠀⠀⠀⠈⠻⣷⣦⣀⠀⠉⠻⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠿⠛⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠛⠷⣦⣤⣀⠀⠈⠉⠉⠙⠛⠛⠛⠉⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠉⠛⠛⠒⠒⠒⠒⠒⠒⠒⠂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀"
            };

            // Center the ASCII art in the bounds
            int artHeight = galaxyArt.Length;
            int artWidth = galaxyArt.Max(line => line.Length);
            int startY = bounds.Y + (bounds.Height - artHeight) / 2;
            int startX = bounds.X + (bounds.Width - artWidth) / 2;
            ConsoleColor[] artColors = new[] { ConsoleColor.DarkGray, ConsoleColor.Gray, ConsoleColor.DarkMagenta };
            Random colorRand = new Random(bounds.GetHashCode() ^ 42);

            selectedSystem = null;
            systemUnderCursor = null;
            for (int y = bounds.Y + 1; y < bounds.Y + bounds.Height - 1; y++)
            {
                for (int x = bounds.X + 1; x < bounds.X + bounds.Width - 1; x++)
                {
                    bool isCursor = (x == cursor.cursorX && y == cursor.cursorY);
                    if (systemPositions.TryGetValue((x, y), out var sys))
                    {
                        bool discovered = sys.Star?.GetType().GetProperty("IsDiscovered")?.GetValue(sys.Star) as bool? ?? true;
                        char symbol = discovered ? 'O' : '?';
                        if (isCursor)
                        {
                            buffer.DrawChar(x, y, symbol, ConsoleColor.Black, discovered ? ConsoleColor.Yellow : ConsoleColor.DarkGray);
                            selectedSystem = sys;
                            systemUnderCursor = sys;
                        }
                        else
                        {
                            buffer.DrawChar(x, y, symbol, ConsoleColor.Yellow, ConsoleColor.Black);
                        }
                    }
                    else if (isCursor)
                    {
                        buffer.DrawChar(x, y, ' ', null, ConsoleColor.Yellow);
                    }
                    else
                    {
                        // Check if this (x, y) is within the ASCII art bounds
                        int ax = x - startX;
                        int ay = y - startY;
                        bool drewArt = false;
                        if (ay >= 0 && ay < galaxyArt.Length && ax >= 0 && ax < galaxyArt[ay].Length)
                        {
                            char ch = galaxyArt[ay][ax];
                            if (ch != ' ')
                            {
                                // Use the same color logic as before
                                ConsoleColor color = artColors[(ax + ay + ch) % artColors.Length];
                                if (ch == '⣀' || ch == '⣤' || ch == '⡇' || ch == '⡏' || ch == '⡏' || ch == '⡏')
                                    color = ConsoleColor.DarkGray;
                                if (ch == '⠒' || ch == '⠦' || ch == '⠙' || ch == '⠷' || ch == '⠉' || ch == '⠛' || ch == '⠋')
                                    color = ConsoleColor.Gray;
                                buffer.DrawChar(x, y, ch, color, ConsoleColor.Black);
                                drewArt = true;
                            }
                        }
                        if (!drewArt)
                        {
                            // Draw starfield background
                            int bx = x - minX, by = y - minY;
                            if (bx >= 0 && bx < bgMap.GetLength(0) && by >= 0 && by < bgMap.GetLength(1))
                            {
                                var (ch, color) = bgMap[bx, by];
                                buffer.DrawChar(x, y, ch, color, ConsoleColor.Black);
                            }
                            else
                            {
                                buffer.DrawChar(x, y, ' ', null, ConsoleColor.Black);
                            }
                        }
                    }
                }
            }
        }

        public static void RenderPlanets(
            IBufferWriter buffer,
            SolarSystem system,
            (int X, int Y, int Width, int Height) bounds,
            (int cursorX, int cursorY) cursor,
            out Planet? planetUnderCursor)
        {
            int planetCount = system.Planets.Count;
            int minX = bounds.X + 1;
            int maxX = bounds.X + bounds.Width - 2;
            int minY = bounds.Y + 1;
            int maxY = bounds.Y + bounds.Height - 2;
            double planetRadiusX = (maxX - minX) / 2.2;
            double planetRadiusY = (maxY - minY) / 2.2;
            int mapCenterX = minX + (maxX - minX) / 2;
            int mapCenterY = minY + (maxY - minY) / 2;
            var planetPositions = new Dictionary<(int X, int Y), Planet>();
            int systemSeed = system.Id;
            var rand = new Random(systemSeed);
            for (int i = 0; i < planetCount; i++)
            {
                double baseAngle = 2 * Math.PI * i / planetCount;
                double angleOffset = (rand.NextDouble() - 0.5) * (Math.PI / planetCount);
                double angle = baseAngle + angleOffset;
                double radiusOffsetX = planetRadiusX * (0.9 + 0.2 * rand.NextDouble());
                double radiusOffsetY = planetRadiusY * (0.9 + 0.2 * rand.NextDouble());
                int x = mapCenterX + (int)(radiusOffsetX * Math.Cos(angle));
                int y = mapCenterY + (int)(radiusOffsetY * Math.Sin(angle));
                x = Math.Max(minX, Math.Min(maxX, x));
                y = Math.Max(minY, Math.Min(maxY, y));
                planetPositions[(x, y)] = system.Planets[i];
                system.Planets[i].X = x;
                system.Planets[i].Y = y;
            }
            // Procedural galaxy-like background
            ConsoleColor[] starColors = new[] { ConsoleColor.DarkGray, ConsoleColor.Gray, ConsoleColor.White, ConsoleColor.Cyan, ConsoleColor.Blue, ConsoleColor.Magenta };
            // Make non-dot stars much less likely
            char[] starChars = new[] { '.', '.', '.', '.', '.', '.', '.', '.', '*', '·', '·', '·', '+', '·' };
            int bgSeed = system.Id ^ 0xBEEF;
            var bgRand = new Random(bgSeed);
            // Precompute a background map for this view
            var bgMap = new (char ch, ConsoleColor color)[maxX - minX + 1, maxY - minY + 1];
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int bx = x - minX, by = y - minY;
                    // Use a deterministic hash for each cell
                    int cellSeed = bgSeed ^ (x * 73856093) ^ (y * 19349663);
                    var cellRand = new Random(cellSeed);
                    double starChance = cellRand.NextDouble();
                    if (starChance < 0.06) // Lowered from 0.18 to 0.06 for fewer stars
                    {
                        char ch = starChars[cellRand.Next(starChars.Length)];
                        ConsoleColor color;
                        if (ch == '*' || ch == '+' || ch == '·')
                        {
                            // Large stars: only white or gray
                            color = (cellRand.Next(2) == 0) ? ConsoleColor.White : ConsoleColor.Gray;
                        }
                        else
                        {
                            // Dots: any color
                            color = starColors[cellRand.Next(starColors.Length)];
                        }
                        bgMap[bx, by] = (ch, color);
                    }
                    else // Empty space
                    {
                        bgMap[bx, by] = (' ', ConsoleColor.Black);
                    }
                }
            }
            planetUnderCursor = null;
            for (int y = bounds.Y + 1; y < bounds.Y + bounds.Height - 1; y++)
            {
                for (int x = bounds.X + 1; x < bounds.X + bounds.Width - 1; x++)
                {
                    bool isCursor = (x == cursor.cursorX && y == cursor.cursorY);
                    if (planetPositions.TryGetValue((x, y), out var planet))
                    {
                        var planetColor = PlanetColors.GetPlanetColor(planet);
                        char planetIcon = GetPlanetIcon(planet);
                        if (isCursor)
                        {
                            buffer.DrawChar(x, y, planetIcon, ConsoleColor.Black, planetColor);
                            planetUnderCursor = planet;
                        }
                        else
                        {
                            buffer.DrawChar(x, y, planetIcon, planetColor, ConsoleColor.Black);
                        }
                    }
                    else if (isCursor)
                    {
                        buffer.DrawChar(x, y, ' ', null, ConsoleColor.Yellow);
                    }
                    else
                    {
                        int bx = x - minX, by = y - minY;
                        if (bx >= 0 && bx < bgMap.GetLength(0) && by >= 0 && by < bgMap.GetLength(1))
                        {
                            var (ch, color) = bgMap[bx, by];
                            buffer.DrawChar(x, y, ch, color, ConsoleColor.Black);
                        }
                        else
                        {
                            buffer.DrawChar(x, y, ' ', null, ConsoleColor.Black);
                        }
                    }
                }
            }
        }
    }
} 