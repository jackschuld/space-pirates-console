using SpacePirates.Console.Core.Interfaces;
using SpacePirates.API.Models;

namespace SpacePirates.Console.UI.ConsoleRenderer
{
    public class ConsoleRenderer : IRenderer
    {
        private readonly int _windowWidth;
        private readonly int _windowHeight;
        
        public ConsoleRenderer(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;
        }

        public void Initialize()
        {
            System.Console.CursorVisible = false;
            System.Console.SetWindowSize(_windowWidth, _windowHeight);
            System.Console.SetBufferSize(_windowWidth, _windowHeight);
            Clear();
        }

        public void Render(IGameState gameState)
        {
            Clear();
            RenderShip(gameState.PlayerShip);
            RenderStats(gameState.PlayerShip);
        }

        public void Clear()
        {
            System.Console.Clear();
        }

        public void DisplayMessage(string message, int x, int y)
        {
            System.Console.SetCursorPosition(x, y);
            System.Console.Write(message);
        }

        private void RenderShip(Ship ship)
        {
            int shipX = (int)ship.Position.X;
            int shipY = (int)ship.Position.Y;
            
            // Simple ship representation
            System.Console.SetCursorPosition(shipX, shipY);
            System.Console.Write(ship.Shield.IsActive ? "⊡" : "□");
        }

        private void RenderStats(Ship ship)
        {
            // Render shield status
            DisplayMessage($"Shield: {ship.Shield.CurrentIntegrity}/{ship.Shield.CalculateMaxCapacity()}", 0, _windowHeight - 3);
            
            // Render fuel status
            DisplayMessage($"Fuel: {ship.FuelSystem.CurrentFuel}/{ship.FuelSystem.CalculateMaxCapacity()}", 0, _windowHeight - 2);
            
            // Render cargo status
            DisplayMessage($"Cargo: {ship.CargoSystem.CurrentLoad}/{ship.CargoSystem.CalculateMaxCapacity()}", 0, _windowHeight - 1);
        }
    }
} 