using SpacePirates.Console.Game;

namespace SpacePirates.Console.UI;

public class GameRenderer
{
    private const char PLAYER_SHIP = '@';
    private const char ENEMY_SHIP = 'E';
    private const char STAR = '*';
    private const char EMPTY = ' ';
    private bool _isInitialized = false;
    private char[,] _previousBuffer = null!; // Will be initialized in Render
    private (int X, int Y) _previousPlayerPos;
    private List<(int X, int Y)> _previousEnemyPos = new();

    public void Render(GameState state)
    {
        if (!_isInitialized)
        {
            InitializeConsole(state.MapSize);
            _previousBuffer = new char[state.MapSize.X, state.MapSize.Y];
            RenderInitialScreen(state);
            _isInitialized = true;
        }

        UpdateChangedPositions(state);
        UpdateStatusBar(state);
    }

    private void InitializeConsole((int X, int Y) mapSize)
    {
        // Set window and buffer size
        int windowWidth = mapSize.X + 2; // Add margins
        int windowHeight = mapSize.Y + 5; // Add space for status bars and menu
        
        try
        {
            if (OperatingSystem.IsWindows())
            {
                // Temporarily expand buffer to prevent scroll issues
                System.Console.BufferWidth = Math.Max(windowWidth, System.Console.BufferWidth);
                System.Console.BufferHeight = Math.Max(windowHeight * 2, System.Console.BufferHeight);
                
                // Set window size
                System.Console.WindowWidth = windowWidth;
                System.Console.WindowHeight = windowHeight;
                
                // Match buffer to window
                System.Console.BufferWidth = windowWidth;
                System.Console.BufferHeight = windowHeight;
            }
        }
        catch (PlatformNotSupportedException)
        {
            // Some platforms don't support window size changes
            System.Console.WriteLine("Warning: Unable to set console window size.");
            System.Threading.Thread.Sleep(1000);
        }

        System.Console.CursorVisible = false;
        System.Console.Clear();
    }

    private void RenderInitialScreen(GameState state)
    {
        // Draw top border
        System.Console.SetCursorPosition(0, 0);
        System.Console.Write('┌' + new string('─', state.MapSize.X) + '┐');

        // Draw side borders and empty space
        for (int y = 0; y < state.MapSize.Y; y++)
        {
            System.Console.SetCursorPosition(0, y + 1);
            System.Console.Write('│');
            System.Console.Write(new string(' ', state.MapSize.X));
            System.Console.Write('│');
        }

        // Draw bottom border
        System.Console.SetCursorPosition(0, state.MapSize.Y + 1);
        System.Console.Write('└' + new string('─', state.MapSize.X) + '┘');

        // Draw stars (they don't move)
        foreach (var star in state.Stars)
        {
            if (IsInBounds(star, state.MapSize))
            {
                System.Console.SetCursorPosition(star.Item1 + 1, star.Item2 + 1);
                System.Console.Write(STAR);
                _previousBuffer[star.Item1, star.Item2] = STAR;
            }
        }

        // Draw initial command menu
        System.Console.SetCursorPosition(0, state.MapSize.Y + 4);
        System.Console.Write("Commands: [↑↓←→] Move | [F] Fire | [S] Shields | [C] Cargo | [Q] Quit");

        // Store initial positions
        _previousPlayerPos = ((int)state.PlayerShip.Position.X, (int)state.PlayerShip.Position.Y);
        _previousEnemyPos = state.EnemyShips.Select(s => ((int)s.Position.X, (int)s.Position.Y)).ToList();
    }

    private void UpdateChangedPositions(GameState state)
    {
        // Clear previous player position
        if (IsInBounds(_previousPlayerPos, state.MapSize))
        {
            System.Console.SetCursorPosition(_previousPlayerPos.Item1 + 1, _previousPlayerPos.Item2 + 1);
            System.Console.Write(EMPTY);
            _previousBuffer[_previousPlayerPos.Item1, _previousPlayerPos.Item2] = EMPTY;
        }

        // Clear previous enemy positions
        foreach (var pos in _previousEnemyPos)
        {
            if (IsInBounds(pos, state.MapSize))
            {
                System.Console.SetCursorPosition(pos.Item1 + 1, pos.Item2 + 1);
                System.Console.Write(EMPTY);
                _previousBuffer[pos.Item1, pos.Item2] = EMPTY;
            }
        }

        // Draw new player position
        var playerPos = ((int)state.PlayerShip.Position.X, (int)state.PlayerShip.Position.Y);
        if (IsInBounds(playerPos, state.MapSize))
        {
            System.Console.SetCursorPosition(playerPos.Item1 + 1, playerPos.Item2 + 1);
            System.Console.Write(PLAYER_SHIP);
            _previousBuffer[playerPos.Item1, playerPos.Item2] = PLAYER_SHIP;
            _previousPlayerPos = playerPos;
        }

        // Draw new enemy positions
        _previousEnemyPos.Clear();
        foreach (var ship in state.EnemyShips)
        {
            var pos = ((int)ship.Position.X, (int)ship.Position.Y);
            if (IsInBounds(pos, state.MapSize))
            {
                System.Console.SetCursorPosition(pos.Item1 + 1, pos.Item2 + 1);
                System.Console.Write(ENEMY_SHIP);
                _previousBuffer[pos.Item1, pos.Item2] = ENEMY_SHIP;
                _previousEnemyPos.Add(pos);
            }
        }
    }

    private void UpdateStatusBar(GameState state)
    {
        var ship = state.PlayerShip;
        var width = state.MapSize.X + 2;
        
        // Clear status area
        for (int i = 2; i <= 4; i++)
        {
            System.Console.SetCursorPosition(0, state.MapSize.Y + i);
            System.Console.Write(new string(' ', width));
        }
        
        // Status bar line 1 - Hull and Shields
        System.Console.SetCursorPosition(0, state.MapSize.Y + 2);
        var hullPercent = (int)((double)ship.Hull.CurrentIntegrity / ship.Hull.CalculateMaxCapacity() * 100);
        var shieldPercent = (int)((double)ship.Shield.CurrentIntegrity / ship.Shield.CalculateMaxCapacity() * 100);
        var shieldStatus = ship.Shield.IsActive ? "ON" : "OFF";
        var statusLine1 = $"Hull: {hullPercent,3}% | Shields: {shieldPercent,3}% [{shieldStatus}] | Position: ({ship.Position.X:F1}, {ship.Position.Y:F1})";
        System.Console.Write(statusLine1.PadRight(width));
        
        // Status bar line 2 - Fuel and Velocity
        System.Console.SetCursorPosition(0, state.MapSize.Y + 3);
        var fuelPercent = (int)((double)ship.FuelSystem.CurrentFuel / ship.FuelSystem.CalculateMaxCapacity() * 100);
        var statusLine2 = $"Fuel: {fuelPercent,3}% | Credits: {ship.Credits:N0}";
        System.Console.Write(statusLine2.PadRight(width));

        // Command menu
        System.Console.SetCursorPosition(0, state.MapSize.Y + 4);
        System.Console.Write("Commands: [↑↓←→] Move | [F] Fire | [S] Shields | [C] Cargo | [Q] Quit".PadRight(width));
    }

    private bool IsInBounds((int Item1, int Item2) pos, (int X, int Y) mapSize)
    {
        return pos.Item1 >= 0 && pos.Item1 < mapSize.X && pos.Item2 >= 0 && pos.Item2 < mapSize.Y;
    }
} 