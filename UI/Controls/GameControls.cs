using SpacePirates.Console.UI.Views;
using SpacePirates.Console.UI.ConsoleRenderer;

namespace SpacePirates.Console.UI.Controls
{
    public class GameControls : BaseControls
    {
        public override void HandleInput(ConsoleKeyInfo key, BaseView view)
        {
            if (IsQuitConfirmActive())
            {
                // Handle quit confirmation keys
                if (key.Key == ConsoleKey.Y)
                {
                    // Actually quit
                    Environment.Exit(0); // or set a flag to stop the game loop
                }
                else if (key.Key == ConsoleKey.N)
                {
                    // Cancel quit confirmation
                    var engine = GetEngine();
                    if (engine != null)
                    {
                        var field = engine.GetType().GetField("_showQuitConfirm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (field != null) field.SetValue(engine, false);
                    }
                    var cr = (SpacePirates.Console.UI.ConsoleRenderer.ConsoleRenderer)GetRenderer();
                    cr.SetHelpText("Tab to toggle instructions | ESC to exit");
                    cr.EndFrame();
                }
                return;
            }

            switch (key.Key)
            {
                case ConsoleKey.Tab:
                    OnTab(view);
                    break;
                case ConsoleKey.Escape:
                    OnEsc(view);
                    break;
                default:
                    // Handle in-game controls (hjkl, etc.) or pass to child
                    break;
            }
        }

        protected virtual void OnTab(BaseView view)
        {
            if (IsQuitConfirmActive()) return;
            var cr = (SpacePirates.Console.UI.ConsoleRenderer.ConsoleRenderer)GetRenderer();
            cr.ShowInstructionsPanel = !cr.ShowInstructionsPanel;
            cr.EndFrame();
        }

        protected virtual void OnEsc(BaseView view)
        {
            if (IsQuitConfirmActive()) return;
            var cr = (SpacePirates.Console.UI.ConsoleRenderer.ConsoleRenderer)GetRenderer();
            cr.SetHelpText("Are you sure you want to quit? (y/n)");
            cr.EndFrame();
            var engine = GetEngine();
            if (engine != null)
            {
                var field = engine.GetType().GetField("_showQuitConfirm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) field.SetValue(engine, true);
            }
        }

        private object GetRenderer()
        {
            return AppDomain.CurrentDomain.GetData("ConsoleRenderer") ?? throw new InvalidOperationException("ConsoleRenderer not found");
        }

        private object? GetEngine()
        {
            return AppDomain.CurrentDomain.GetData("GameEngine");
        }

        private bool IsQuitConfirmActive()
        {
            var engine = GetEngine();
            if (engine == null) return false;
            var field = engine.GetType().GetField("_showQuitConfirm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field != null && (bool)field.GetValue(engine)!;
        }
    }
} 