using System;
using System.Collections.Generic;

namespace SpacePirates.Console.Core.Models.Galaxy
{
    public class SolarSystem
    {
        public int MapX { get; }
        public int MapY { get; }
        public string Name { get; }
        public int Seed { get; }
        public Dictionary<string, object> State { get; }

        public SolarSystem(int mapX, int mapY, int seed)
        {
            MapX = mapX;
            MapY = mapY;
            Seed = seed;
            Name = GenerateName(seed, mapX, mapY);
            State = new Dictionary<string, object>();
        }

        private string GenerateName(int seed, int x, int y)
        {
            // Simple name generator based on seed and coordinates
            var rand = new Random(seed + x * 1000 + y);
            string[] syllables = { "Al", "Be", "Cor", "Den", "Er", "Fi", "Gal", "Hel", "Ion", "Jor", "Kel", "Lum", "Mor", "Nor", "Or", "Pra", "Qua", "Rho", "Sol", "Tor", "Uva", "Vex", "Wol", "Xan", "Yor", "Zen" };
            string name = syllables[rand.Next(syllables.Length)] + syllables[rand.Next(syllables.Length)] + rand.Next(10, 99);
            return name;
        }

        public string[] GetAsciiArt()
        {
            // Compact ASCII art: sun and up to one planet
            var rand = new Random(Seed);
            int numPlanets = rand.Next(0, 2);
            var art = new List<string> { "O" };
            if (numPlanets == 1) art.Add("o");
            return art.ToArray();
        }
    }
} 