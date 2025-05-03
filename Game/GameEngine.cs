using SpacePirates.Console.UI;
using SpacePirates.API.Models;

namespace SpacePirates.Console.Game;

public class GameEngine
{
    private bool _isRunning;
    private readonly GameRenderer _renderer;
    private readonly InputHandler _inputHandler;
    private GameState _gameState;
    private const double MOVEMENT_SPEED = 0.5;
    private const double FUEL_COST_PER_MOVE = 0.25;
    private const double INERTIA_FACTOR = 0.95; // How much velocity is retained
    private const double MAX_VELOCITY = 2.0;
    private double _velocityX = 0;
    private double _velocityY = 0;
    private DateTime _lastUpdate = DateTime.UtcNow;

    public GameEngine()
    {
        _renderer = new GameRenderer();
        _inputHandler = new InputHandler();
        _gameState = new GameState();
        _isRunning = false;
    }

    public void Start()
    {
        System.Console.WriteLine("Game engine starting...");
        _isRunning = true;
        RunGameLoop();
    }

    private void RunGameLoop()
    {
        System.Console.WriteLine("Entering game loop...");
        System.Threading.Thread.Sleep(1000); // Give time to see debug message

        const int targetFps = 60;
        const int targetFrameTime = 1000 / targetFps;
        var frameTimer = new System.Diagnostics.Stopwatch();

        while (_isRunning)
        {
            try
            {
                frameTimer.Restart();

                // Calculate delta time
                var now = DateTime.UtcNow;
                var deltaTime = (now - _lastUpdate).TotalSeconds;
                _lastUpdate = now;

                // Process Input
                var command = _inputHandler.GetCommand();
                ProcessCommand(command);

                // Update Game State
                UpdateGameState(deltaTime);

                // Render
                _renderer.Render(_gameState);

                // Cap frame rate
                frameTimer.Stop();
                var elapsedMs = frameTimer.ElapsedMilliseconds;
                if (elapsedMs < targetFrameTime)
                {
                    System.Threading.Thread.Sleep((int)(targetFrameTime - elapsedMs));
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error in game loop: {ex.Message}");
                System.Console.WriteLine(ex.StackTrace);
                System.Console.WriteLine("Press any key to exit...");
                System.Console.ReadKey();
                _isRunning = false;
            }
        }
    }

    private void ProcessCommand(Command command)
    {
        switch (command.Type)
        {
            case CommandType.Exit:
                _isRunning = false;
                break;
            case CommandType.Move:
                if (command.Data is Direction direction)
                {
                    ApplyThrust(direction);
                }
                break;
            case CommandType.Attack:
                // TODO: Implement combat
                break;
            case CommandType.ToggleShields:
                ToggleShields();
                break;
            case CommandType.ViewCargo:
                ViewCargo();
                break;
        }
    }

    private void ApplyThrust(Direction direction)
    {
        var ship = _gameState.PlayerShip;
        
        // Check if we have enough fuel
        if (ship.FuelSystem.CurrentFuel <= 0)
        {
            return; // Can't move without fuel
        }

        // Calculate thrust based on engine efficiency
        double thrust = MOVEMENT_SPEED * ship.Engine.CurrentLevel;
        double fuelCost = FUEL_COST_PER_MOVE / ship.FuelSystem.Efficiency;

        // Apply thrust based on direction
        switch (direction)
        {
            case Direction.Up:
                _velocityY = Math.Max(-MAX_VELOCITY, _velocityY - thrust);
                break;
            case Direction.Down:
                _velocityY = Math.Min(MAX_VELOCITY, _velocityY + thrust);
                break;
            case Direction.Left:
                _velocityX = Math.Max(-MAX_VELOCITY, _velocityX - thrust);
                break;
            case Direction.Right:
                _velocityX = Math.Min(MAX_VELOCITY, _velocityX + thrust);
                break;
        }

        // Consume fuel when thrusting
        ship.FuelSystem.CurrentFuel = (int)Math.Max(0, ship.FuelSystem.CurrentFuel - fuelCost);
    }

    private void UpdateGameState(double deltaTime)
    {
        var ship = _gameState.PlayerShip;

        // Apply inertia
        _velocityX *= Math.Pow(INERTIA_FACTOR, deltaTime * 60); // Scale with frame rate
        _velocityY *= Math.Pow(INERTIA_FACTOR, deltaTime * 60);

        // Update position
        double newX = ship.Position.X + (_velocityX * deltaTime * 60);
        double newY = ship.Position.Y + (_velocityY * deltaTime * 60);

        // Constrain to map boundaries with bounce
        if (newX < 0)
        {
            newX = 0;
            _velocityX = Math.Abs(_velocityX) * 0.5; // Bounce with reduced velocity
        }
        else if (newX >= _gameState.MapSize.X)
        {
            newX = _gameState.MapSize.X - 1;
            _velocityX = -Math.Abs(_velocityX) * 0.5;
        }

        if (newY < 0)
        {
            newY = 0;
            _velocityY = Math.Abs(_velocityY) * 0.5;
        }
        else if (newY >= _gameState.MapSize.Y)
        {
            newY = _gameState.MapSize.Y - 1;
            _velocityY = -Math.Abs(_velocityY) * 0.5;
        }

        ship.Position.X = newX;
        ship.Position.Y = newY;

        // Update other game systems
        _gameState.Update();
    }

    private void ToggleShields()
    {
        var shield = _gameState.PlayerShip.Shield;
        if (shield.CurrentIntegrity > 0)
        {
            shield.IsActive = !shield.IsActive;
            var status = shield.IsActive ? "activated" : "deactivated";
            System.Console.SetCursorPosition(0, _gameState.MapSize.Y + 3);
            System.Console.Write($"Shields {status}!".PadRight(_gameState.MapSize.X + 2));
        }
        else
        {
            System.Console.SetCursorPosition(0, _gameState.MapSize.Y + 3);
            System.Console.Write("Shields are depleted!".PadRight(_gameState.MapSize.X + 2));
        }
    }

    private void ViewCargo()
    {
        var cargo = _gameState.PlayerShip.CargoSystem;
        var maxCapacity = cargo.CalculateMaxCapacity();
        var usedSpace = cargo.CurrentLoad;
        var freeSpace = maxCapacity - usedSpace;
        
        // Clear status area
        for (int i = 2; i <= 4; i++)
        {
            System.Console.SetCursorPosition(0, _gameState.MapSize.Y + i);
            System.Console.Write(new string(' ', _gameState.MapSize.X + 2));
        }
        
        // Display cargo information
        System.Console.SetCursorPosition(0, _gameState.MapSize.Y + 2);
        System.Console.Write($"Cargo Capacity: {usedSpace}/{maxCapacity} units ({freeSpace} units free)".PadRight(_gameState.MapSize.X + 2));
        System.Console.SetCursorPosition(0, _gameState.MapSize.Y + 3);
        System.Console.Write($"Cargo Level: {cargo.CurrentLevel} (Upgrade cost: {cargo.UpgradeCost} credits)".PadRight(_gameState.MapSize.X + 2));
    }
} 