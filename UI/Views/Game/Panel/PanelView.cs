using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.Core.Interfaces;

namespace SpacePirates.Console.UI.Views
{
    public class PanelView : BaseView
    {
        public BoxStyle BorderStyle { get; set; } = BoxStyle.Double;

        public PanelView(BaseControls controls, BaseStyle styleProvider)
        {
            Controls = controls ?? throw new ArgumentNullException(nameof(controls));
            StyleProvider = styleProvider ?? throw new ArgumentNullException(nameof(styleProvider));
        }

        public override void Render()
        {
            // Render panel using StyleProvider
        }

        public override void HandleInput(ConsoleKeyInfo key)
        {
            if (Controls != null)
                Controls.HandleInput(key, this);
        }

        public virtual void Update(IGameState gameState) { }
        public virtual void Render(IBufferWriter buffer) { }
    }
} 