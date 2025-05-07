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

            // Show ship name and captain name at the top
            buffer.DrawString(textX, y++, $"Ship: {ship.Name} | Captain: {ship.CaptainName}", ConsoleColor.White, ConsoleColor.Black);

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
            string hullBar = StatusComponentHelpers.RenderBar(hullPercentage, 12, '■', ' ');
            buffer.DrawString(textX, y++, $"Hull:   {hullBar}", ConsoleColor.DarkGreen);

            // SHIELD
            if (ship.Shield != null)
            {
                var shieldPercentage = (ship.Shield.CurrentIntegrity / (float)ship.Shield.CalculateMaxCapacity()) * 100;
                int barWidth = 12;
                string bar = StatusComponentHelpers.RenderBar(ship.Shield.IsActive || ship.Shield.Charging ? shieldPercentage : 0, barWidth, '■', ' ');
                if (ship.Shield.IsActive || ship.Shield.Charging)
                    buffer.DrawString(textX, y++, $"Shield: {bar}", ConsoleColor.DarkCyan);
                else
                    buffer.DrawString(textX, y++, $"Shield: {bar}", ConsoleColor.DarkGray);
            }

            // CARGO
            if (ship.CargoSystem != null)
            {
                var cargoPercentage = (ship.CargoSystem.CurrentLoad / (float)ship.CargoSystem.CalculateMaxCapacity()) * 100;
                string cargoBar = StatusComponentHelpers.RenderBar(cargoPercentage, 12, '■', ' ');
                buffer.DrawString(textX, y++, $"Cargo:  {cargoBar}", ConsoleColor.DarkYellow);
            }
            
            // FUEL
            if (ship.FuelSystem != null)
            {
                var fuelPercentage = (ship.FuelSystem.CurrentFuel / ship.FuelSystem.CalculateMaxCapacity()) * 100.0;
                string fuelBar = StatusComponentHelpers.RenderBar(fuelPercentage, 12, '■', ' ');
                string fuelLine = $"Fuel:   {fuelBar}";
                if (ship.Shield != null && (ship.Shield.IsActive || ship.Shield.Charging))
                    fuelLine += "  ";
                buffer.DrawString(textX, y++, fuelLine, ConsoleColor.Red);
                if (ship.Shield != null && (ship.Shield.IsActive || ship.Shield.Charging))
                    buffer.DrawString(textX + fuelLine.Length + 1, y - 1, "-5%", ConsoleColor.Blue);
            }

            // Divider before weapons
            buffer.DrawString(textX, y++, new string('─', _bounds.Width - 4), PanelStyles.FadedColor);

            // WEAPONS
            if (ship.WeaponSystem != null)
            {
                buffer.DrawString(textX, y++, "WEAPONS:", PanelStyles.SubtitleColor);
                // Damage as a bar (assuming max 100 for bar)
                double damagePercent = Math.Min(100.0, (ship.WeaponSystem.Damage / 100.0) * 100.0);
                string damageBar = StatusComponentHelpers.RenderBar(damagePercent, 12, '■', ' ');
                buffer.DrawString(textX, y++, $"Damage: {damageBar}", ConsoleColor.DarkRed);
                buffer.DrawString(textX, y++, $"Accuracy: ", ConsoleColor.DarkCyan);
                buffer.DrawString(textX, y++, $"Crit: ", ConsoleColor.Magenta);
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
            "Move: x y or xy",
        };
        public static readonly (string Key, string Description)[] QuickKeys = new[]
        {
            ("m", "Start a Move command"),
            ("Tab", "Toggle this panel (Status/Instructions)"),
            ("ESC", "Quit game")
        };
        public const string Header = "INSTRUCTIONS";
    }

    public class InstructionsPanelComponent : IStatusComponent
    {
        private readonly (int X, int Y, int Width, int Height) _bounds;
        private readonly SpacePirates.Console.Game.Engine.GameEngine.ControlState? _controlState;

        public InstructionsPanelComponent(int x, int y, int width, int height)
        {
            _bounds = (x, y, width, height);
        }
        public InstructionsPanelComponent(int x, int y, int width, int height, SpacePirates.Console.Game.Engine.GameEngine.ControlState controlState)
        {
            _bounds = (x, y, width, height);
            _controlState = controlState;
        }

        public (int X, int Y, int Width, int Height) Bounds => _bounds;

        public void Render(IBufferWriter buffer)
        {
            buffer.Clear(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height);
            PanelRenderer.DrawPanelFrameWithTab(buffer, _bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, InstructionsData.Header, PanelStyles.TitleColor);
            int y = _bounds.Y + 3;
            int textX = _bounds.X + 2;

            // Select instructions and quick keys based on control state
            string[] commands;
            (string Key, string Description)[] quickKeys;
            switch (_controlState)
            {
                case SpacePirates.Console.Game.Engine.GameEngine.ControlState.GalaxyMap:
                    commands = new[] { "Move: h/j/k/l", "Warp: w + System ID" };
                    quickKeys = new[] {
                        ("h/j/k/l", "Move cursor"),
                        ("w", "Start a Warp command"),
                        ("Tab", "Toggle this panel (Status/Instructions)"),
                        ("ESC", "Quit game")
                    };
                    break;
                case SpacePirates.Console.Game.Engine.GameEngine.ControlState.SolarSystemView:
                    commands = new[] { "Move: x y or xy" };
                    quickKeys = new[] {
                        ("m", "Start a Move command"),
                        ("s", "Toggle shield"),
                        ("Tab", "Toggle this panel (Status/Instructions)"),
                        ("ESC", "Quit game")
                    };
                    break;
                case SpacePirates.Console.Game.Engine.GameEngine.ControlState.StartMenu:
                    commands = new[] { "Move: h/j/k/l", "Select: Enter" };
                    quickKeys = new[] {
                        ("h/j/k/l", "Move selection"),
                        ("Enter", "Select option")
                    };
                    break;
                default:
                    commands = InstructionsData.Commands;
                    quickKeys = InstructionsData.QuickKeys;
                    break;
            }

            // Commands section
            buffer.DrawString(textX, y++, "COMMANDS:", PanelStyles.SubtitleColor);
            foreach (var desc in commands)
            {
                buffer.DrawString(textX, y++, desc, ConsoleColor.DarkGreen);
            }
            y++;
            // Divider
            buffer.DrawString(textX, y++, new string('─', _bounds.Width - 4), PanelStyles.FadedColor);
            // Quick keys section
            buffer.DrawString(textX, y++, "QUICK KEYS:", PanelStyles.SubtitleColor);
            foreach (var (key, desc) in quickKeys)
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

    public static class StatusComponentHelpers
    {
        public static string RenderBar(double percent, int width, char fillChar, char emptyChar)
        {
            int filled = (int)(width * (percent / 100.0));
            filled = Math.Clamp(filled, 0, width);
            return "[" + new string(fillChar, filled) + new string(emptyChar, width - filled) + "]";
        }
    }
} 