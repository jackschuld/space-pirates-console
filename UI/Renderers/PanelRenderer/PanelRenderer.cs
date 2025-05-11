using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.UI.Components;

namespace SpacePirates.Console.UI.Renderers.PanelRenderer
{
    public static class PanelRenderer
    {
        public static void DrawPanelFrameWithTab(IBufferWriter buffer, int x, int y, int width, int height, string title, ConsoleColor titleColor, BoxStyle borderStyle)
        {
            // Draw border in gold/darkyellow if possible
            if (buffer is SpacePirates.Console.UI.ConsoleRenderer.ConsoleBufferWriter cbw)
                cbw.DrawBox(x, y, width, height, borderStyle, PanelStyles.BorderColor);
            else
                buffer.DrawBox(x, y, width, height, borderStyle);
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