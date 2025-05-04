using SpacePirates.Console.Core.Interfaces;
using SpacePirates.API.Models;

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

            // Draw status border
            buffer.DrawBox(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, BoxStyle.Single);

            if (_gameState?.PlayerShip == null) return;

            var ship = _gameState.PlayerShip;
            int currentY = _bounds.Y + 2;
            
            // Ship Status Section
            buffer.DrawString(_bounds.X + 2, currentY, "Ship Status", ConsoleColor.White);
            currentY += 2;

            // Ship Name and Position
            buffer.DrawString(_bounds.X + 2, currentY, $"Name: {ship.Name}", ConsoleColor.Yellow);
            currentY++;
            buffer.DrawString(_bounds.X + 2, currentY, $"Position: ({ship.Position.X:F1}, {ship.Position.Y:F1})", ConsoleColor.Gray);
            currentY += 2;

            // Hull Status
            var hullPercentage = (ship.Hull.CurrentIntegrity / (float)ship.Hull.CalculateMaxCapacity()) * 100;
            var hullColor = hullPercentage > 66 ? ConsoleColor.Green :
                           hullPercentage > 33 ? ConsoleColor.Yellow :
                           ConsoleColor.Red;
            buffer.DrawString(_bounds.X + 2, currentY, $"Hull: {hullPercentage:F1}%", hullColor);
            buffer.DrawString(_bounds.X + 2, currentY + 1, $"Level: {ship.Hull.CurrentLevel}", ConsoleColor.Gray);
            currentY += 2;

            // Shield Status
            if (ship.Shield != null)
            {
                var shieldPercentage = (ship.Shield.CurrentIntegrity / (float)ship.Shield.CalculateMaxCapacity()) * 100;
                buffer.DrawString(_bounds.X + 2, currentY, $"Shield: {shieldPercentage:F1}%", ConsoleColor.Cyan);
                buffer.DrawString(_bounds.X + 2, currentY + 1, $"Status: {(ship.Shield.IsActive ? "ACTIVE" : "OFF")}", 
                    ship.Shield.IsActive ? ConsoleColor.Green : ConsoleColor.Gray);
                buffer.DrawString(_bounds.X + 2, currentY + 2, $"Level: {ship.Shield.CurrentLevel}", ConsoleColor.Gray);
                currentY += 3;
            }

            // Fuel Status
            if (ship.FuelSystem != null)
            {
                var fuelPercentage = (ship.FuelSystem.CurrentLevel / (float)ship.FuelSystem.CalculateMaxCapacity()) * 100;
                buffer.DrawString(_bounds.X + 2, currentY, $"Fuel: {fuelPercentage:F1}%", ConsoleColor.Blue);
                buffer.DrawString(_bounds.X + 2, currentY + 1, $"Level: {ship.FuelSystem.CurrentLevel}", ConsoleColor.Gray);
                currentY += 2;
            }

            // Cargo Status
            if (ship.CargoSystem != null)
            {
                var cargoPercentage = (ship.CargoSystem.CurrentLoad / (float)ship.CargoSystem.CalculateMaxCapacity()) * 100;
                buffer.DrawString(_bounds.X + 2, currentY, $"Cargo: {cargoPercentage:F1}%", ConsoleColor.DarkYellow);
                buffer.DrawString(_bounds.X + 2, currentY + 1, $"Level: {ship.CargoSystem.CurrentLevel}", ConsoleColor.Gray);
                currentY += 2;
            }

            // Weapon Status
            if (ship.WeaponSystem != null)
            {
                buffer.DrawString(_bounds.X + 2, currentY, "Weapons", ConsoleColor.White);
                buffer.DrawString(_bounds.X + 2, currentY + 1, $"Level: {ship.WeaponSystem.CurrentLevel}", ConsoleColor.Gray);
                buffer.DrawString(_bounds.X + 2, currentY + 2, $"Damage: {ship.WeaponSystem.Damage:F0}", ConsoleColor.Red);
                buffer.DrawString(_bounds.X + 2, currentY + 3, $"Accuracy: {ship.WeaponSystem.Accuracy:P0}", ConsoleColor.Yellow);
                buffer.DrawString(_bounds.X + 2, currentY + 4, $"Crit: {ship.WeaponSystem.CriticalChance:P0}", ConsoleColor.Magenta);
                currentY += 5;
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
} 