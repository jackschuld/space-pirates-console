namespace SpacePirates.Console.UI.Interfaces
{
    public interface IView
    {
        void Render();
        void HandleInput(ConsoleKeyInfo key);
    }
} 