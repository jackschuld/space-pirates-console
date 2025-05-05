using SpacePirates.API.Models;
using System;
using System.Collections.Generic;

namespace SpacePirates.Console.Core.Models.Movement
{
    public class MovementSystem
    {
        private const double INERTIA_FACTOR = 0.95;
        private const double MAX_VELOCITY = 2.0;
        
        private double _velocityX;
        private double _velocityY;
        
        public void ApplyThrust(Position position, double thrust, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    _velocityY = Math.Max(-MAX_VELOCITY, _velocityY - thrust);
                    break;
                case Direction.Down:
                    _velocityY = Math.Min(MAX_VELOCITY, _velocityY + thrust);
                    break;
                case Direction.Left:
                    _velocityX = Math.Max(-MAX_VELOCITY, _velocityX - thrust);
                    break;
                case Direction.Right:
                    _velocityX = Math.Min(MAX_VELOCITY, _velocityX + thrust);
                    break;
            }
        }
        
        public void Update(Position position, double deltaTime, Position mapSize)
        {
            // Apply inertia
            _velocityX *= Math.Pow(INERTIA_FACTOR, deltaTime * 60);
            _velocityY *= Math.Pow(INERTIA_FACTOR, deltaTime * 60);

            // Update position
            double newX = position.X + (_velocityX * deltaTime * 60);
            double newY = position.Y + (_velocityY * deltaTime * 60);

            // Handle boundaries
            HandleBoundaryCollision(ref newX, ref _velocityX, 0, mapSize.X);
            HandleBoundaryCollision(ref newY, ref _velocityY, 0, mapSize.Y);

            position.X = (int)newX;
            position.Y = (int)newY;
        }

        private void HandleBoundaryCollision(ref double position, ref double velocity, double min, double max)
        {
            if (position < min)
            {
                position = min;
                velocity = Math.Abs(velocity) * 0.5; // Bounce with reduced velocity
            }
            else if (position >= max)
            {
                position = max - 1;
                velocity = -Math.Abs(velocity) * 0.5;
            }
        }

        public void MoveShipTo(Ship ship, int targetX, int targetY, Action onStep, Action<string> onMessage, ShipTrail? trail = null)
        {
            if (ship == null)
                return;

            if (ship.IsDestroyed)
            {
                onMessage?.Invoke("Ship is destroyed and cannot move.");
                return;
            }

            int startX = (int)ship.Position.X;
            int startY = (int)ship.Position.Y;

            double baseSpeed = Math.Max(1.0, ship.Engine.MaxSpeed);
            double cargoPenalty = 1.0 - ((double)ship.CargoSystem.CurrentLoad / Math.Max(1, ship.CargoSystem.CalculateMaxCapacity()));
            double fuelEfficiency = ship.FuelSystem.Efficiency;
            double effectiveSpeed = baseSpeed * cargoPenalty * fuelEfficiency;
            effectiveSpeed = Math.Max(0.5, effectiveSpeed); // Clamp to minimum speed (0.5 units/sec)

            double fuelCostPerStep = (1.0 / fuelEfficiency) / 4.0;

            int dx = targetX - startX;
            int dy = targetY - startY;
            int totalSteps = Math.Max(Math.Abs(dx), Math.Abs(dy));
            if (totalSteps == 0) totalSteps = 1;

            int step = 0;
            bool reachedTarget = false;
            while (step <= totalSteps)
            {
                if (ship.IsDestroyed)
                {
                    onMessage?.Invoke($"[Step {step}] Ship destroyed during movement at ({ship.Position.X:F1}, {ship.Position.Y:F1})!");
                    break;
                }
                if (ship.FuelSystem.CurrentFuel <= 0)
                {
                    onMessage?.Invoke($"[Step {step}] Fuel empty. Did not reach target. Final position: ({ship.Position.X:F1}, {ship.Position.Y:F1}), Target: ({targetX}, {targetY})");
                    return;
                }

                double t = (double)step / totalSteps;
                double x = startX + t * dx;
                double y = startY + t * dy;
                // Tighter S/figure-8: higher freq, lower amplitude
                double angle = Math.Atan2(dy, dx);
                double amplitude = 1.0 + 0.5 * Math.Abs(Math.Sin(Math.PI * t));
                double freq = 6.0 * Math.PI; // Tighter
                double offset = Math.Sin(freq * t) * amplitude * Math.Cos(Math.PI * t);
                double perpX = -Math.Sin(angle);
                double perpY = Math.Cos(angle);
                x += perpX * offset;
                y += perpY * offset;

                int curX = (int)Math.Round(x);
                int curY = (int)Math.Round(y);
                if ((dx > 0 && curX > targetX) || (dx < 0 && curX < targetX)) curX = targetX;
                if ((dy > 0 && curY > targetY) || (dy < 0 && curY < targetY)) curY = targetY;

                ship.Position.X = curX;
                ship.Position.Y = curY;
                ship.FuelSystem.CurrentFuel -= (int)Math.Ceiling(fuelCostPerStep);
                if (ship.FuelSystem.CurrentFuel < 0) ship.FuelSystem.CurrentFuel = 0;

                // Update trail
                trail?.AddPoint(curX, curY);

                onMessage?.Invoke($"[Step {step}] Ship at ({curX}, {curY}), Target: ({targetX}, {targetY}), Fuel: {ship.FuelSystem.CurrentFuel}");
                onStep?.Invoke();

                int msDelay = (int)(2000.0 - (effectiveSpeed * 500.0));
                msDelay = Math.Clamp(msDelay, 1000, 2000);
                System.Threading.Thread.Sleep(msDelay);
                step++;

                if (curX == targetX && curY == targetY)
                {
                    reachedTarget = true;
                    break;
                }
            }
            if (reachedTarget)
            {
                onMessage?.Invoke($"Arrived at ({targetX}, {targetY}) after {step} steps.");
            }
            else if (ship.FuelSystem.CurrentFuel <= 0)
            {
                onMessage?.Invoke($"Fuel empty. Did not reach target. Final position: ({ship.Position.X:F1}, {ship.Position.Y:F1}), Target: ({targetX}, {targetY})");
            }
            else
            {
                onMessage?.Invoke($"Did not reach target. Final position: ({ship.Position.X:F1}, {ship.Position.Y:F1}), Target: ({targetX}, {targetY})");
            }
        }
    }

    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }
} 