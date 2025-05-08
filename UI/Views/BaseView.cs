using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.UI.Interfaces;

namespace SpacePirates.Console.UI.Views
{
    public abstract class BaseView : IView
    {
        public BaseControls? Controls { get; protected set; }
        public BaseStyleProvider? StyleProvider { get; protected set; }

        public abstract void Render();
        public abstract void HandleInput(ConsoleKeyInfo key);
    }
} 