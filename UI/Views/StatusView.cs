using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.UI.Components;

namespace SpacePirates.Console.UI.Views
{
    public abstract class StatusView : PanelView
    {
        protected readonly string _title;
        protected (int X, int Y, int Width, int Height) _bounds;

        public StatusView(BaseControls controls, StatusPanelStyle styleProvider, (int X, int Y, int Width, int Height) bounds, string title)
            : base(controls, styleProvider)
        {
            _bounds = bounds;
            _title = title;
        }

        public (int X, int Y, int Width, int Height) Bounds => _bounds;

        public override void Render()
        {
            // You would pass a buffer in the real render, but for now, this is a stub
        }

        public override void Render(IBufferWriter buffer)
        {
            buffer.Clear(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height);
            PanelRenderer.DrawPanelFrameWithTab(buffer, _bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, _title, PanelStyles.TitleColor);
            int y = _bounds.Y + 3;
            int textX = _bounds.X + 2;
            RenderDetails(buffer, textX, ref y);
        }

        protected abstract void RenderDetails(IBufferWriter buffer, int textX, ref int y);

        public override void Update(IGameState gameState) { }
        public virtual void UpdateStatus(IGameState gameState) { }
    }

    public static class PanelRenderer
    {
        public static void DrawPanelFrameWithTab(IBufferWriter buffer, int x, int y, int width, int height, string title, ConsoleColor titleColor)
        {
            // Draw border in gold/darkyellow if possible
            if (buffer is SpacePirates.Console.UI.ConsoleRenderer.ConsoleBufferWriter cbw)
                cbw.DrawBox(x, y, width, height, BoxStyle.Double, PanelStyles.BorderColor);
            else
                buffer.DrawBox(x, y, width, height, BoxStyle.Double);
            // Draw title in all caps at top left
            string titleText = title.ToUpperInvariant();
            buffer.DrawString(x + 2, y + 1, titleText, titleColor);
            // Draw Tab indicator at bottom right
            string tabLabel = "Tab";
            string icon = "⇄ ";
            int tabX = x + width - (tabLabel.Length + icon.Length) - 2;
            int tabY = y + height - 2;
            buffer.DrawString(tabX, tabY, icon, PanelStyles.CommandInactiveColor);
            buffer.DrawString(tabX + icon.Length, tabY, tabLabel, PanelStyles.QuickKeyColor);
        }
    }
} 