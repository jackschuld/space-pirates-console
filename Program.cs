using SpacePirates.Console.Game.Engine;
using SpacePirates.Console.UI.ConsoleRenderer;

namespace SpacePirates.Console;

public class Program
{
    private const int WINDOW_WIDTH = 120;  // Increased width for better layout
    private const int WINDOW_HEIGHT = 45;  // Increased height to ensure all content is visible

    public static void Main(string[] args)
    {
        try
        {
            // Create renderer
            var renderer = new ConsoleRenderer();

            // Create and initialize game engine
            var engine = new GameEngine(renderer);
            engine.Initialize();

            // Run the game
            engine.Run();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Fatal error: {ex.Message}");
            System.Console.WriteLine(ex.StackTrace);
            System.Console.WriteLine("\nPress any key to exit...");
            System.Console.ReadKey(true);
        }
    }
}
