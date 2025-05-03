using SpacePirates.Console.Game;

namespace SpacePirates.Console;

public class Program
{
    public static void Main(string[] args)
    {
        // Clear the console and hide the cursor
        System.Console.Clear();
        System.Console.CursorVisible = false;

        // Initialize and start the game
        var game = new GameEngine();
        game.Start();

        // Cleanup when game exits
        System.Console.CursorVisible = true;
        System.Console.Clear();
    }
}
