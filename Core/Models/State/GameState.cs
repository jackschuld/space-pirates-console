using System;
using SpacePirates.API.Models;
using SpacePirates.API.Models.ShipComponents;
using SpacePirates.Console.Core.Interfaces;

namespace SpacePirates.Console.Core.Models.State
{
    public class GameState : IGameState
    {
        public Ship PlayerShip { get; private set; } = null!;
        public Position MapSize { get; private set; } = null!;
        public Galaxy Galaxy { get; private set; } = null!;
        private const int HELP_AREA_HEIGHT = 3;

        public GameState()
        {
            // Use the precalculated usable game area dimensions
            MapSize = new Position 
            { 
                X = ConsoleConfig.UsableGameWidth,
                Y = ConsoleConfig.UsableGameHeight
            };
            
            InitializePlayerShip();
        }

        public GameState(Ship loadedShip)
        {
            MapSize = new Position
            {
                X = ConsoleConfig.UsableGameWidth,
                Y = ConsoleConfig.UsableGameHeight
            };
            PlayerShip = loadedShip;

            // Defensive: ensure all critical components are not null
            if (PlayerShip.Position == null)
                PlayerShip.Position = new Position { X = MapSize.X / 2, Y = MapSize.Y / 2 };
            if (PlayerShip.Hull == null)
                PlayerShip.Hull = new Hull { CurrentLevel = 1, CurrentIntegrity = 100 };
            if (PlayerShip.Shield == null)
                PlayerShip.Shield = new Shield { CurrentLevel = 1, CurrentIntegrity = 100, IsActive = false };
            if (PlayerShip.Engine == null)
                PlayerShip.Engine = new Engine { CurrentLevel = 1 };
            if (PlayerShip.FuelSystem == null)
                PlayerShip.FuelSystem = new FuelSystem { CurrentLevel = 1 };
            if (PlayerShip.CargoSystem == null)
                PlayerShip.CargoSystem = new CargoSystem { CurrentLevel = 1, CurrentLoad = 0 };
            if (PlayerShip.WeaponSystem == null)
                PlayerShip.WeaponSystem = new WeaponSystem { CurrentLevel = 1 };
        }

        public GameState(Ship loadedShip, Galaxy loadedGalaxy)
        {
            MapSize = new Position
            {
                X = ConsoleConfig.UsableGameWidth,
                Y = ConsoleConfig.UsableGameHeight
            };
            PlayerShip = loadedShip;
            Galaxy = loadedGalaxy;

            // Defensive: ensure all critical components are not null
            if (PlayerShip.Position == null)
                PlayerShip.Position = new Position { X = MapSize.X / 2, Y = MapSize.Y / 2 };
            if (PlayerShip.Hull == null)
                PlayerShip.Hull = new Hull { CurrentLevel = 1, CurrentIntegrity = 100 };
            if (PlayerShip.Shield == null)
                PlayerShip.Shield = new Shield { CurrentLevel = 1, CurrentIntegrity = 100, IsActive = false };
            if (PlayerShip.Engine == null)
                PlayerShip.Engine = new Engine { CurrentLevel = 1 };
            if (PlayerShip.FuelSystem == null)
                PlayerShip.FuelSystem = new FuelSystem { CurrentLevel = 1 };
            if (PlayerShip.CargoSystem == null)
                PlayerShip.CargoSystem = new CargoSystem { CurrentLevel = 1, CurrentLoad = 0 };
            if (PlayerShip.WeaponSystem == null)
                PlayerShip.WeaponSystem = new WeaponSystem { CurrentLevel = 1 };
        }

        private void InitializePlayerShip()
        {
            PlayerShip = new Ship
            {
                Name = "Player Ship",
                Position = new Position
                {
                    X = MapSize.X / 2,
                    Y = MapSize.Y / 2
                },
                Hull = new Hull 
                { 
                    CurrentLevel = 1,
                    CurrentIntegrity = 100
                },
                Shield = new Shield 
                { 
                    CurrentLevel = 1,
                    CurrentIntegrity = 100,
                    IsActive = false
                },
                Engine = new Engine 
                { 
                    CurrentLevel = 1 
                },
                FuelSystem = new FuelSystem 
                { 
                    CurrentLevel = 1
                },
                CargoSystem = new CargoSystem 
                { 
                    CurrentLevel = 1,
                    CurrentLoad = 0
                },
                WeaponSystem = new WeaponSystem 
                { 
                    CurrentLevel = 1
                }
            };
        }

        public void Update()
        {
            if (PlayerShip == null)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("[ERROR] PlayerShip is null in GameState.Update().");
                System.Console.ResetColor();
                return;
            }
            if (PlayerShip.Position == null)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("[ERROR] PlayerShip.Position is null in GameState.Update().");
                System.Console.ResetColor();
                return;
            }
            if (MapSize == null)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("[ERROR] MapSize is null in GameState.Update().");
                System.Console.ResetColor();
                return;
            }
            if (PlayerShip.Shield == null)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("[ERROR] PlayerShip.Shield is null in GameState.Update().");
                System.Console.ResetColor();
                // Not returning, since shield is optional for some logic
            }

            // Update ship systems
            PlayerShip.Shield?.Recharge();
            
            // Constrain ship position to game area
            PlayerShip.Position.X = Math.Max(1, Math.Min(PlayerShip.Position.X, MapSize.X));
            PlayerShip.Position.Y = Math.Max(1, Math.Min(PlayerShip.Position.Y, MapSize.Y));
        }
    }
} 