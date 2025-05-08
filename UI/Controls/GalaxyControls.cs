using SpacePirates.Console.UI.Views;

namespace SpacePirates.Console.UI.Controls
{
    public class GalaxyControls : GameControls
    {
        public override void HandleInput(ConsoleKeyInfo key, BaseView view)
        {
            // Handle galaxy-specific controls (warp, system select, etc.)
            base.HandleInput(key, view);
        }
    }
} 