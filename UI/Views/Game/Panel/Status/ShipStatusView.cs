using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.Core.Interfaces;
using SpacePirates.API.Models;
using System;
using SpacePirates.Console.UI.Components;
using SpacePirates.Console.UI.Views;

namespace SpacePirates.Console.UI.Views
{
    public class ShipStatusView : StatusView
    {
        private IGameState? _gameState;

        public ShipStatusView(BaseControls controls, StatusPanelStyle styleProvider, (int X, int Y, int Width, int Height) bounds)
            : base(controls, styleProvider, bounds, "SHIP STATUS") { }

        protected override void RenderDetails(IBufferWriter buffer, int textX, ref int y)
        {
            if (_gameState?.PlayerShip == null)
            {
                buffer.DrawString(textX, y++, "No ship data available.", PanelStyles.CommandTextColor);
                return;
            }

            var ship = _gameState.PlayerShip;
            string shipName = TruncateWithEllipsis(ship.Name, 10);
            string captainName = TruncateWithEllipsis(ship.CaptainName, 10);
            buffer.DrawString(textX, y++, $"Ship: {shipName} | Captain: {captainName}", PanelStyles.CommandTextColor);

            int posLabelX = textX + 16;
            buffer.DrawString(textX, y, "NAME:", PanelStyles.SubtitleColor);
            buffer.DrawString(posLabelX, y, "POSITION:", PanelStyles.SubtitleColor);
            y++;
            buffer.DrawString(textX, y, shipName, PanelStyles.CommandTextColor);
            buffer.DrawString(posLabelX, y, $"({ship.Position.X:F1}, {ship.Position.Y:F1})", PanelStyles.CommandTextColor);
            y++;

            buffer.DrawString(textX, y++, new string('─', 24), PanelStyles.FadedColor);
            buffer.DrawString(textX, y++, "SHIP:", PanelStyles.SubtitleColor);

            var hullPercentage = (ship.Hull.CurrentIntegrity / (float)ship.Hull.CalculateMaxCapacity()) * 100;
            string hullBar = ShipStatusHelper.RenderBar(hullPercentage, 12, '■', ' ');
            buffer.DrawString(textX, y++, $"Hull:   {hullBar}", ConsoleColor.DarkGreen);

            if (ship.Shield != null)
            {
                var shieldPercentage = (ship.Shield.CurrentIntegrity / (float)ship.Shield.CalculateMaxCapacity()) * 100;
                int barWidth = 12;
                string bar = ShipStatusHelper.RenderBar(ship.Shield.IsActive || ship.Shield.Charging ? shieldPercentage : 0, barWidth, '■', ' ');
                if (ship.Shield.IsActive || ship.Shield.Charging)
                    buffer.DrawString(textX, y++, $"Shield: {bar}", ConsoleColor.DarkCyan);
                else
                    buffer.DrawString(textX, y++, $"Shield: {bar}", ConsoleColor.DarkGray);
            }

            if (ship.CargoSystem != null)
            {
                var cargoPercentage = (ship.CargoSystem.CurrentLoad / (float)ship.CargoSystem.CalculateMaxCapacity()) * 100;
                string cargoBar = ShipStatusHelper.RenderBar(cargoPercentage, 12, '■', ' ');
                buffer.DrawString(textX, y++, $"Cargo:  {cargoBar}", ConsoleColor.DarkYellow);
            }

            if (ship.FuelSystem != null)
            {
                var fuelPercentage = (ship.FuelSystem.CurrentFuel / ship.FuelSystem.CalculateMaxCapacity()) * 100.0;
                string fuelBar = ShipStatusHelper.RenderBar(fuelPercentage, 12, '■', ' ');
                string fuelLine = $"Fuel:   {fuelBar}";
                if (ship.Shield != null && (ship.Shield.IsActive || ship.Shield.Charging))
                    fuelLine += "  ";
                buffer.DrawString(textX, y++, fuelLine, ConsoleColor.Red);
                if (ship.Shield != null && (ship.Shield.IsActive || ship.Shield.Charging))
                    buffer.DrawString(textX + fuelLine.Length + 1, y - 1, "-5%", ConsoleColor.Blue);
            }

            buffer.DrawString(textX, y++, new string('─', 24), PanelStyles.FadedColor);

            if (ship.WeaponSystem != null)
            {
                buffer.DrawString(textX, y++, "WEAPONS:", PanelStyles.SubtitleColor);
                double damagePercent = Math.Min(100.0, (ship.WeaponSystem.Damage / 100.0) * 100.0);
                string damageBar = ShipStatusHelper.RenderBar(damagePercent, 12, '■', ' ');
                buffer.DrawString(textX, y++, $"Damage: {damageBar}", ConsoleColor.DarkRed);
                buffer.DrawString(textX, y++, $"Accuracy: ", ConsoleColor.DarkCyan);
                buffer.DrawString(textX, y++, $"Crit: ", ConsoleColor.Magenta);
            }
        }

        public override void Update(IGameState gameState)
        {
            _gameState = gameState;
        }

        public override void UpdateStatus(IGameState gameState)
        {
            _gameState = gameState;
        }

        private static string TruncateWithEllipsis(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength) return value;
            return value.Substring(0, maxLength - 1) + "…";
        }
    }
} 