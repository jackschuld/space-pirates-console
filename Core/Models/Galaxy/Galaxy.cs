using System;
using System.Collections.Generic;

namespace SpacePirates.Console.Core.Models.Galaxy
{
    public class Galaxy
    {
        public const int Width = 70; // character width of the galaxy map area
        public const int Height = 28; // character height of the galaxy map area
        public List<SolarSystem> Systems { get; }
        public int Seed { get; }

        public Galaxy(int seed, int numSystems = 20)
        {
            Seed = seed;
            Systems = new List<SolarSystem>();
            var rand = new Random(seed);
            for (int i = 0; i < numSystems; i++)
            {
                int x = rand.Next(3, Width - 3); // leave margin for art
                int y = rand.Next(2, Height - 2);
                int systemSeed = rand.Next();
                Systems.Add(new SolarSystem(x, y, systemSeed));
            }
        }

        public SolarSystem? GetSystemAt(int x, int y, int tolerance = 2)
        {
            // Find a system within a small radius of (x, y)
            foreach (var sys in Systems)
            {
                if (Math.Abs(sys.MapX - x) <= tolerance && Math.Abs(sys.MapY - y) <= tolerance)
                    return sys;
            }
            return null;
        }
    }
} 