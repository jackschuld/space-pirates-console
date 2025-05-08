using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.Core.Interfaces;

namespace SpacePirates.Console.UI.Views
{
    public class PanelView : BaseView
    {
        public PanelView(BaseControls controls, BaseStyleProvider styleProvider)
        {
            Controls = controls;
            StyleProvider = styleProvider;
        }

        public override void Render()
        {
            // Render panel using StyleProvider
        }

        public override void HandleInput(ConsoleKeyInfo key)
        {
            Controls.HandleInput(key, this);
        }

        public virtual void Update(IGameState gameState) { }
        public virtual void Render(IBufferWriter buffer) { }
    }
} 