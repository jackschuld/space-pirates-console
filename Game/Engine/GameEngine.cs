using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Core.Models.Movement;
using SpacePirates.Console.Core.Models.State;
using SpacePirates.API.Models;

namespace SpacePirates.Console.Game.Engine
{
    public class GameEngine
    {
        private readonly IRenderer _renderer;
        private IGameState? _gameState;
        private bool _isRunning;

        public GameEngine(IRenderer renderer)
        {
            _renderer = renderer;
            _isRunning = false;
        }

        public void Initialize()
        {
            _renderer.Initialize(120, 30); // Standard terminal size
            _gameState = new GameState(); // Initialize with default state
            _isRunning = true;
        }

        public void Run()
        {
            while (_isRunning)
            {
                ProcessInput();
                Update();
                Render();
                Thread.Sleep(16); // ~60 FPS
            }
        }

        private void ProcessInput()
        {
            if (System.Console.KeyAvailable)
            {
                var key = System.Console.ReadKey(true);
                _renderer.HandleInput(key);

                if (key.Key == ConsoleKey.Escape)
                {
                    _isRunning = false;
                }
            }
        }

        private void Update()
        {
            if (_gameState == null) return;

            // Update game state
            _gameState.Update();
            
            // Update renderer with new state
            _renderer.UpdateGameState(_gameState);
        }

        private void Render()
        {
            _renderer.BeginFrame();
            _renderer.EndFrame();
        }

        public void Stop()
        {
            _isRunning = false;
        }
    }
} 