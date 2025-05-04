using SpacePirates.API.Models;

namespace SpacePirates.Console.Core.Interfaces
{
    public interface IGameState
    {
        Ship PlayerShip { get; }
        Position MapSize { get; }
        void Update();
    }
} 