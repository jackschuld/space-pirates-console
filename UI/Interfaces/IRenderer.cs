using SpacePirates.API.Models;

namespace SpacePirates.Console.Core.Interfaces
{
    public interface IRenderer : IDisposable
    {
        // Core functionality
        void Initialize();
        void BeginFrame();
        void EndFrame();
        void Clear();

        // Component interaction methods
        void HandleInput(ConsoleKeyInfo keyInfo);
        void UpdateGameState(IGameState gameState);
        void SetHelpText(string text);
        void ShowMessage(string message, bool temporary = false);
    }

    public interface IUIComponent
    {
        void Render(IBufferWriter buffer);
        void Update(IGameState gameState);
        (int X, int Y, int Width, int Height) Bounds { get; }
    }

    public interface IBufferWriter
    {
        void DrawChar(int x, int y, char character, ConsoleColor? foreground = null, ConsoleColor? background = null);
        void DrawString(int x, int y, string text, ConsoleColor? foreground = null, ConsoleColor? background = null);
        void DrawBox(int x, int y, int width, int height, BoxStyle style = BoxStyle.Single);
        void Clear(int x, int y, int width, int height);
        bool IsInBounds(int x, int y);
    }

    public interface IGameComponent : IUIComponent
    {
        void HandleInput(ConsoleKeyInfo keyInfo);
    }

    public interface IStatusComponent : IUIComponent
    {
        void UpdateStatus(IGameState gameState);
    }

    public interface IHelpComponent : IUIComponent
    {
        void SetHelpText(string text);
    }

    public enum BoxStyle
    {
        Single,
        Double,
        Rounded
    }

    public enum UIRegion
    {
        Game,
        Status,
        Help
    }
} 