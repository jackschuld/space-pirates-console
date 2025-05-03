namespace SpacePirates.Console.Core.Interfaces
{
    public interface IGameEngine
    {
        void Start();
        void Stop();
        void Update(double deltaTime);
        bool IsRunning { get; }
    }
} 