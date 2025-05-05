using SpacePirates.Console.Core.Interfaces;
using SpacePirates.API.Models;
using SpacePirates.Console.UI.InputHandling.CommandSystem;

namespace SpacePirates.Console.UI.Components
{
    public class StatusComponent : IStatusComponent
    {
        private readonly (int X, int Y, int Width, int Height) _bounds;
        private IGameState? _gameState;

        public StatusComponent(int x, int y, int width, int height)
        {
            _bounds = (x, y, width, height);
        }

        public (int X, int Y, int Width, int Height) Bounds => _bounds;

        public void Render(IBufferWriter buffer)
        {
            // Clear status area
            buffer.Clear(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height);

            // Draw border, Tab indicator, and title
            PanelRenderer.DrawPanelFrameWithTab(buffer, _bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, "SHIP STATUS", PanelStyles.TitleColor);
            int y = _bounds.Y + 3; // Add a blank line after the title
            int textX = _bounds.X + 2;

            if (_gameState?.PlayerShip == null) return;

            var ship = _gameState.PlayerShip;

            // NAME and POSITION on the same line, values below
            int posLabelX = textX + 16;
            buffer.DrawString(textX, y, "NAME:", PanelStyles.SubtitleColor);
            buffer.DrawString(posLabelX, y, "POSITION:", PanelStyles.SubtitleColor);
            y++;
            buffer.DrawString(textX, y, ship.Name, ConsoleColor.White);
            buffer.DrawString(posLabelX, y, $"({ship.Position.X:F1}, {ship.Position.Y:F1})", ConsoleColor.White);
            y++;
            
            buffer.DrawString(textX, y++, new string('─', _bounds.Width - 4), PanelStyles.FadedColor);
            buffer.DrawString(textX, y++, "SHIP:", PanelStyles.SubtitleColor);

            // HULL
            var hullPercentage = (ship.Hull.CurrentIntegrity / (float)ship.Hull.CalculateMaxCapacity()) * 100;
            buffer.DrawString(textX, y, $"Hull: {hullPercentage:F1}%", ConsoleColor.DarkGreen);
            buffer.DrawString(textX + 20, y++, $"--level:{ship.Hull.CurrentLevel}", PanelStyles.FadedColor);

            // SHIELD
            if (ship.Shield != null)
            {
                var shieldPercentage = (ship.Shield.CurrentIntegrity / (float)ship.Shield.CalculateMaxCapacity()) * 100;
                buffer.DrawString(textX, y, $"Shield: {shieldPercentage:F1}%", ConsoleColor.DarkCyan);
                buffer.DrawString(textX + 20, y++, $"--level:{ship.Shield.CurrentLevel}", PanelStyles.FadedColor);
            }

            // CARGO
            if (ship.CargoSystem != null)
            {
                var cargoPercentage = (ship.CargoSystem.CurrentLoad / (float)ship.CargoSystem.CalculateMaxCapacity()) * 100;
                buffer.DrawString(textX, y, $"Cargo: {cargoPercentage:F1}%", ConsoleColor.DarkYellow);
                buffer.DrawString(textX + 20, y++, $"--level:{ship.CargoSystem.CurrentLevel}", PanelStyles.FadedColor);
            }
            
            // FUEL
            if (ship.FuelSystem != null)
            {
                var fuelPercentage = (ship.FuelSystem.CurrentLevel / (float)ship.FuelSystem.CalculateMaxCapacity()) * 100;
                buffer.DrawString(textX, y, $"Fuel: {fuelPercentage:F1}%", ConsoleColor.Red);
                buffer.DrawString(textX + 20, y++, $"--level:{ship.FuelSystem.CurrentLevel}", PanelStyles.FadedColor);
            }

            // Divider before weapons
            buffer.DrawString(textX, y++, new string('─', _bounds.Width - 4), PanelStyles.FadedColor);

            // WEAPONS
            if (ship.WeaponSystem != null)
            {
                buffer.DrawString(textX, y++, "WEAPONS:", PanelStyles.SubtitleColor);
                buffer.DrawString(textX, y, $"Damage: {ship.WeaponSystem.Damage:F0}", ConsoleColor.DarkRed);
                buffer.DrawString(textX + 20, y++, $"--level:{ship.WeaponSystem.CurrentLevel}", PanelStyles.FadedColor);
                buffer.DrawString(textX, y++, $"Accuracy: {ship.WeaponSystem.Accuracy:P0}", ConsoleColor.DarkCyan);
                buffer.DrawString(textX, y++, $"Crit: {ship.WeaponSystem.CriticalChance:P0}", ConsoleColor.Magenta);
            }
        }

        public void Update(IGameState gameState)
        {
            _gameState = gameState;
        }

        public void UpdateStatus(IGameState gameState)
        {
            _gameState = gameState;
        }
    }

    public static class InstructionsData
    {
        public static readonly string[] Commands = new[]
        {
            MoveCommand.Description
        };
        public static readonly (string Key, string Description)[] QuickKeys = new[]
        {
            ("c", "Enter command mode"),
            ("Tab", "Toggle this panel (Status/Instructions)"),
            ("ESC", "Quit game")
        };
        public const string Header = "INSTRUCTIONS";
    }

    public class InstructionsPanelComponent : IStatusComponent
    {
        private readonly (int X, int Y, int Width, int Height) _bounds;

        public InstructionsPanelComponent(int x, int y, int width, int height)
        {
            _bounds = (x, y, width, height);
        }

        public (int X, int Y, int Width, int Height) Bounds => _bounds;

        public void Render(IBufferWriter buffer)
        {
            buffer.Clear(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height);
            PanelRenderer.DrawPanelFrameWithTab(buffer, _bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, InstructionsData.Header, PanelStyles.TitleColor);
            int y = _bounds.Y + 3;
            int textX = _bounds.X + 2;
            // Commands section
            buffer.DrawString(textX, y++, "COMMANDS:", PanelStyles.SubtitleColor);
            foreach (var desc in InstructionsData.Commands)
            {
                buffer.DrawString(textX, y++, desc, ConsoleColor.DarkGreen);
            }
            y++;
            // Divider
            buffer.DrawString(textX, y++, new string('─', _bounds.Width - 4), PanelStyles.FadedColor);
            // Quick keys section
            buffer.DrawString(textX, y++, "QUICK KEYS:", PanelStyles.SubtitleColor);
            foreach (var (key, desc) in InstructionsData.QuickKeys)
            {
                y = DrawStyled(buffer, textX, y, key, desc, _bounds.Width - 4, PanelStyles.SubtitleColor, ConsoleColor.Gray, false, true);
            }
        }

        private int DrawStyled(IBufferWriter buffer, int x, int y, string left, string right, int maxWidth, ConsoleColor leftColor, ConsoleColor rightColor, bool isCommand = false, bool isQuickKey = false)
        {
            string leftPart = left;
            string rightPart = right;
            int leftLen = leftPart.Length + 2; // +2 for ' - '
            var leftCol = isCommand ? ConsoleColor.DarkGreen : isQuickKey ? ConsoleColor.Cyan : leftColor;
            if (leftLen + rightPart.Length > maxWidth)
            {
                buffer.DrawString(x, y, leftPart + " - ", leftCol);
                y = DrawWrapped(buffer, x + leftLen, y, rightPart, maxWidth - leftLen, rightColor);
            }
            else
            {
                buffer.DrawString(x, y, leftPart, leftCol);
                buffer.DrawString(x + leftPart.Length, y, " - ", ConsoleColor.DarkGray);
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

        public void Update(IGameState gameState) { }
        public void UpdateStatus(IGameState gameState) { }
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
            buffer.DrawString(tabX, tabY, icon, ConsoleColor.Gray);
            buffer.DrawString(tabX + icon.Length, tabY, tabLabel, ConsoleColor.Cyan);
        }
    }

    public static class PanelStyles
    {
        public const ConsoleColor BorderColor = ConsoleColor.DarkYellow; 
        public const ConsoleColor TitleColor = ConsoleColor.White;
        public const ConsoleColor SubtitleColor = ConsoleColor.Yellow;
        public const ConsoleColor FadedColor = ConsoleColor.DarkGray;
    }
} 