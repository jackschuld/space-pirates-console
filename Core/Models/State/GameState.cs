using SpacePirates.API.Models;
using SpacePirates.API.Models.ShipComponents;
using SpacePirates.Console.Core.Interfaces;

namespace SpacePirates.Console.Core.Models.State
{
    public class GameState : IGameState
    {
        public Ship PlayerShip { get; private set; } = null!;
        public Position MapSize { get; private set; } = null!;
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
            // Update ship systems
            PlayerShip.Shield?.Recharge();
            
            // Constrain ship position to game area
            PlayerShip.Position.X = Math.Max(1, Math.Min(PlayerShip.Position.X, MapSize.X));
            PlayerShip.Position.Y = Math.Max(1, Math.Min(PlayerShip.Position.Y, MapSize.Y));
        }
    }
} 