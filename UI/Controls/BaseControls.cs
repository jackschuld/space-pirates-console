using SpacePirates.Console.UI.Views;

namespace SpacePirates.Console.UI.Controls
{
    public abstract class BaseControls
    {
        public abstract void HandleInput(ConsoleKeyInfo key, BaseView view);
    }
} 