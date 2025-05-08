using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;

namespace SpacePirates.Console.UI.Views
{
    public class MenuView : BaseView
    {
        public MenuView(MenuControls controls, BaseStyleProvider styleProvider)
        {
            Controls = controls;
            StyleProvider = styleProvider;
        }

        public override void Render()
        {
            // Render menu options using StyleProvider
        }

        public override void HandleInput(ConsoleKeyInfo key)
        {
            Controls.HandleInput(key, this);
        }
    }
} 