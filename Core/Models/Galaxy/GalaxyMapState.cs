using System.Collections.Generic;

namespace SpacePirates.Console.Core.Models.Galaxy
{
    public class GalaxyMapState
    {
        public int CursorX { get; set; }
        public int CursorY { get; set; }
        public HashSet<(int x, int y)> VisitedSystems { get; }

        public GalaxyMapState()
        {
            CursorX = 0;
            CursorY = 0;
            VisitedSystems = new HashSet<(int x, int y)>();
        }

        public void Visit(int x, int y)
        {
            VisitedSystems.Add((x, y));
        }

        public bool IsVisited(int x, int y)
        {
            return VisitedSystems.Contains((x, y));
        }
    }
} 