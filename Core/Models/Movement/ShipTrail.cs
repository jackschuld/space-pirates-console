using System.Collections.Generic;

namespace SpacePirates.Console.Core.Models.Movement
{
    public class ShipTrail
    {
        private readonly int _maxLength;
        private readonly Queue<(int x, int y)> _trail;

        public ShipTrail(int maxLength = 12)
        {
            _maxLength = maxLength;
            _trail = new Queue<(int x, int y)>();
        }

        public void AddPoint(int x, int y)
        {
            if (_trail.Count > 0 && _trail.Peek() == (x, y))
                return; // Don't add duplicate consecutive points
            _trail.Enqueue((x, y));
            while (_trail.Count > _maxLength)
                _trail.Dequeue();
        }

        public IReadOnlyList<(int x, int y)> GetTrail()
        {
            return new List<(int x, int y)>(_trail);
        }
    }
} 