using SpacePirates.API.Models;

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

            position.X = newX;
            position.Y = newY;
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