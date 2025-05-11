using SpacePirates.Console.Game;
using SpacePirates.Console.UI.ConsoleRenderer;
using SpacePirates.Console.UI.Components;
using System;
using System.Threading.Tasks;
using SpacePirates.API.Models;
using SpacePirates.API.Models.ShipComponents;
using SpacePirates.Console.UI.Views;
using SpacePirates.Console.Game.Engine;

namespace SpacePirates.Console;

public class Program
{
    private const int WINDOW_WIDTH = 120;  // Increased width for better layout
    private const int WINDOW_HEIGHT = 45;  // Increased height to ensure all content is visible

    public static void Main(string[] args)
    {
        try
        {
            RunAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Fatal error: {ex.Message}");
            System.Console.WriteLine(ex.StackTrace);
            System.Console.WriteLine("\nPress any key to exit...");
            System.Console.ReadKey(true);
        }
    }

    private static async Task RunAsync()
    {
        string apiBaseUrl = Environment.GetEnvironmentVariable("SPACEPIRATES_API_URL") ?? "http://localhost:5139";
        var api = new ApiClient(apiBaseUrl);
        AppDomain.CurrentDomain.SetData("ApiClient", api);
        System.Console.WriteLine("[DEBUG] ApiClient registered in AppDomain");
        var savedGames = await api.ListGamesAsync();
        bool hasSave = savedGames.Count > 0;

        // Show start menu (disable Load if no saves)
        var startMenu = new StartMenuView(hasSave);
        startMenu.Show();

        switch (startMenu.SelectedOption)
        {
            case StartMenuView.MenuOption.StartNewGame:
                if (hasSave)
                {
                    // Overwrite: delete old save
                    await api.DeleteGameAsync(savedGames[0].gameId);
                }
                System.Console.Write("Enter captain name: ");
                var captain = System.Console.ReadLine() ?? "Captain";
                System.Console.Write("Enter ship name: ");
                var ship = System.Console.ReadLine() ?? "Ship";
                var newGame = await api.StartNewGameAsync(captain, ship);
                if (newGame == null || newGame.GetShip() == null || newGame.GetGalaxy() == null)
                {
                    System.Console.WriteLine("Failed to start new game.");
                    return;
                }
                var renderer = new ConsoleRenderer();
                AppDomain.CurrentDomain.SetData("ConsoleRenderer", renderer);
                var gameState = MapToCoreGameState(newGame);
                var engine = new GameEngine(renderer);
                AppDomain.CurrentDomain.SetData("GameEngine", engine);
                engine.Initialize(gameState);
                engine.Run();
                break;
            case StartMenuView.MenuOption.LoadGame:
                if (!hasSave)
                {
                    System.Console.WriteLine("No saved games to load.");
                    return;
                }
                // For now, just load the first save
                var game = await api.LoadGameAsync(savedGames[0].gameId);
                if (game == null || game.GetShip() == null)
                {
                    System.Console.WriteLine("Failed to load game.");
                    return;
                }
                var renderer2 = new ConsoleRenderer();
                AppDomain.CurrentDomain.SetData("ConsoleRenderer", renderer2);
                var gameState2 = MapToCoreGameState(game);
                var engine2 = new GameEngine(renderer2);
                AppDomain.CurrentDomain.SetData("GameEngine", engine2);
                engine2.Initialize(gameState2);
                engine2.Run();
                break;
            case StartMenuView.MenuOption.Quit:
                System.Console.WriteLine("Goodbye!");
                return;
        }
    }

    private static SpacePirates.Console.Core.Models.State.GameState MapToCoreGameState(GameStateDto dto)
    {
        if (dto.ship == null || dto.galaxy == null)
            throw new InvalidOperationException("GameStateDto is missing ship or galaxy");
        return new SpacePirates.Console.Core.Models.State.GameState(dto.ship, dto.galaxy);
    }
}
