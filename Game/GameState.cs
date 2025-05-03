using SpacePirates.API.Models;
using SpacePirates.API.Models.ShipComponents;

namespace SpacePirates.Console.Game;

public class GameState
{
    public Ship PlayerShip { get; private set; }
    public List<Ship> EnemyShips { get; private set; }
    public (int X, int Y) MapSize { get; private set; }
    public List<(int X, int Y)> Stars { get; private set; }

    public GameState()
    {
        MapSize = (80, 24); // Default console size
        PlayerShip = CreateDefaultPlayerShip();
        EnemyShips = new List<Ship>();
        Stars = GenerateStars();
    }

    public void Update()
    {
        // Update all game entities
        UpdatePlayerShip();
        UpdateEnemyShips();
        CheckCollisions();
    }

    private Ship CreateDefaultPlayerShip()
    {
        var position = new Position 
        { 
            X = MapSize.X / 2, 
            Y = MapSize.Y / 2 
        };

        var ship = new Ship
        {
            Name = "Player Ship",
            Position = position,
            Hull = new Hull { CurrentLevel = 2 },
            Shield = new Shield { CurrentLevel = 2 },
            Engine = new Engine { CurrentLevel = 2 },
            FuelSystem = new FuelSystem { CurrentLevel = 2 },
            CargoSystem = new CargoSystem { CurrentLevel = 1 },
            WeaponSystem = new WeaponSystem { CurrentLevel = 1 },
            Credits = 1000
        };

        // Set up navigation properties
        position.Ship = ship;
        position.ShipId = 1; // Arbitrary ID for the player ship

        // Initialize component states
        ship.Hull.CurrentIntegrity = ship.Hull.CalculateMaxCapacity();
        ship.Shield.CurrentIntegrity = ship.Shield.CalculateMaxCapacity();
        ship.FuelSystem.CurrentFuel = ship.FuelSystem.CalculateMaxCapacity();
        ship.CargoSystem.CurrentLoad = 0;

        return ship;
    }

    private List<(int X, int Y)> GenerateStars()
    {
        var random = new Random();
        var stars = new List<(int X, int Y)>();
        int numStars = (MapSize.X * MapSize.Y) / 50; // Roughly 2% of the map

        for (int i = 0; i < numStars; i++)
        {
            stars.Add((
                random.Next(0, MapSize.X),
                random.Next(0, MapSize.Y)
            ));
        }

        return stars;
    }

    private void UpdatePlayerShip()
    {
        // Update shield recharge
        PlayerShip.Shield.Recharge();
    }

    private void UpdateEnemyShips()
    {
        foreach (var ship in EnemyShips)
        {
            ship.Shield.Recharge();
        }
    }

    private void CheckCollisions()
    {
        // Check for collisions between ships
        // TODO: Implement collision detection
    }
} 