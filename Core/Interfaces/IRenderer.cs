namespace SpacePirates.Console.Core.Interfaces
{
    public interface IRenderer
    {
        void Initialize();
        void Render(IGameState gameState);
        void Clear();
        void DisplayMessage(string message, int x, int y);
    }
} 