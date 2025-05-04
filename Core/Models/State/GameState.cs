using SpacePirates.API.Models;
using SpacePirates.API.Models.ShipComponents;
using SpacePirates.Console.Core.Interfaces;

namespace SpacePirates.Console.Core.Models.State
{
    public class GameState : IGameState
    {
        public Ship PlayerShip { get; private set; } = null!;
        public Position MapSize { get; private set; } = null!;
        private const int STATUS_AREA_HEIGHT = 6;

        public GameState(int mapWidth = 80, int mapHeight = 24)
        {
            // Adjust map height to account for status area
            mapHeight = mapHeight - STATUS_AREA_HEIGHT;
            
            MapSize = new Position { X = mapWidth - 2, Y = mapHeight - 2 }; // Account for borders
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
                    CurrentLevel = 1,
                    CurrentFuel = 100
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