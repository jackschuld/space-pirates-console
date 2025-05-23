using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Game.Engine;
using SpacePirates.Console.UI.Views;
using System.Linq;
using SpacePirates.Console.UI.Renderers.PanelRenderer;
using SpacePirates.Console.UI.Views.Game.Panel.Instructions;

namespace SpacePirates.Console.UI.Views
{
    public class InstructionsView : PanelView
    {
        private readonly (int X, int Y, int Width, int Height) _bounds;
        private readonly SpacePirates.Console.Game.Engine.GameEngine.ControlState? _controlState;

        public InstructionsView(BaseControls controls, InstructionsPanelStyle styleProvider, (int X, int Y, int Width, int Height) bounds, SpacePirates.Console.Game.Engine.GameEngine.ControlState? controlState = null)
            : base(controls, styleProvider)
        {
            _bounds = bounds;
            _controlState = controlState;
        }

        public (int X, int Y, int Width, int Height) Bounds => _bounds;

        public override void Update(IGameState gameState) { }
        public override void Render(IBufferWriter buffer)
        {
            buffer.Clear(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height);
            PanelRenderer.DrawPanelFrameWithTab(buffer, _bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, InstructionsData.Header, PanelStyles.TitleColor, BorderStyle);
            int y = _bounds.Y + 3;
            int textX = _bounds.X + 2;

            // Use explicit types for deconstruction
            (string[] commands, (string Key, string Description)[] quickKeys) = InstructionsProvider.GetInstructions(_controlState);

            // Commands section
            buffer.DrawString(textX, y++, "COMMANDS:", PanelStyles.SubtitleColor);
            foreach (var desc in commands)
            {
                buffer.DrawString(textX, y++, desc, PanelStyles.CommandTextColor);
            }
            y++;
            // Divider
            buffer.DrawString(textX, y++, new string('─', _bounds.Width - 4), PanelStyles.FadedColor);
            // Quick keys section
            buffer.DrawString(textX, y++, "QUICK KEYS:", PanelStyles.SubtitleColor);
            // Use explicit types for deconstruction in foreach
            foreach ((string key, string desc) in quickKeys)
            {
                y = DrawStyled(buffer, textX, y, key, desc, _bounds.Width - 4, PanelStyles.QuickKeyColor, PanelStyles.CommandTextColor, false, true);
            }
        }

        private int DrawStyled(IBufferWriter buffer, int x, int y, string left, string right, int maxWidth, ConsoleColor leftColor, ConsoleColor rightColor, bool isCommand = false, bool isQuickKey = false)
        {
            string leftPart = left;
            string rightPart = right;
            int leftLen = leftPart.Length + 2; // +2 for ' - '
            var leftCol = isCommand ? PanelStyles.CommandTextColor : isQuickKey ? PanelStyles.QuickKeyColor : leftColor;
            if (leftLen + rightPart.Length > maxWidth)
            {
                buffer.DrawString(x, y, leftPart + " - ", leftCol);
                y = DrawWrapped(buffer, x + leftLen, y, rightPart, maxWidth - leftLen, rightColor);
            }
            else
            {
                buffer.DrawString(x, y, leftPart, leftCol);
                buffer.DrawString(x + leftPart.Length, y, " - ", PanelStyles.FadedColor);
                buffer.DrawString(x + leftPart.Length + 3, y, rightPart, rightColor);
                y++;
            }
            return y;
        }

        private int DrawWrapped(IBufferWriter buffer, int x, int y, string text, int maxWidth, ConsoleColor color)
        {
            while (text.Length > maxWidth)
            {
                int split = text.LastIndexOf(' ', Math.Min(maxWidth, text.Length - 1));
                if (split <= 0) split = maxWidth;
                buffer.DrawString(x, y++, text.Substring(0, split), color);
                text = text.Substring(split).TrimStart();
            }
            buffer.DrawString(x, y++, text, color);
            return y;
        }
    }
} 