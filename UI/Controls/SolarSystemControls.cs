using SpacePirates.Console.UI.Views;

namespace SpacePirates.Console.UI.Controls
{
    public class SolarSystemControls : GameControls
    {
        public override void HandleInput(ConsoleKeyInfo key, BaseView view)
        {
            // Handle solar system-specific controls (planet select, etc.)
            base.HandleInput(key, view);
        }
    }
} 