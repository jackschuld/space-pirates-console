using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Core.Models.Movement;
using SpacePirates.Console.Core.Models.State;
using SpacePirates.API.Models;
using SpacePirates.Console.UI.InputHandling.CommandSystem;
using System.Text;

namespace SpacePirates.Console.Game.Engine
{
    public class GameEngine
    {
        private readonly IRenderer _renderer;
        private IGameState? _gameState;
        private bool _isRunning;
        private CommandParser? _commandParser;
        private MoveCommand? _moveCommand;
        private string _defaultHelpText = "Type ':' to enter command mode | ESC to exit";
        private string _lastCommandResult = string.Empty;

        public GameEngine(IRenderer renderer)
        {
            _renderer = renderer;
            _isRunning = false;
        }

        public void Initialize()
        {
            _renderer.Initialize();  // Use ConsoleConfig values instead of hardcoded dimensions
            _gameState = new GameState(); // Initialize with default state
            _isRunning = true;

            // Setup command system
            var context = new CommandContext(this, _renderer);
            _moveCommand = new MoveCommand();
            _commandParser = new CommandParser(context, new[] { _moveCommand });

            // Set initial help text
            _renderer.SetHelpText(_defaultHelpText);
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
            if (!System.Console.KeyAvailable) return;
            var key = System.Console.ReadKey(true);

            if (key.KeyChar == ':')
            {
                var commandBuffer = new StringBuilder(":");
                _renderer.SetHelpText(commandBuffer.ToString());
                _renderer.EndFrame(); // Force UI update

                while (true)
                {
                    var cmdKey = System.Console.ReadKey(true);
                    if (cmdKey.Key == ConsoleKey.Enter) break;
                    if (cmdKey.Key == ConsoleKey.Backspace && commandBuffer.Length > 1)
                    {
                        commandBuffer.Length--;
                    }
                    else if (!char.IsControl(cmdKey.KeyChar))
                    {
                        commandBuffer.Append(cmdKey.KeyChar);
                    }
                    _renderer.SetHelpText(commandBuffer.ToString());
                    _renderer.EndFrame(); // Force UI update after each key
                }

                var commandInput = commandBuffer.ToString().Substring(1); // Remove initial ':'
                var result = _commandParser?.ParseAndExecuteWithResult(":" + commandInput);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    _renderer.SetHelpText(result);
                }
                else
                {
                    _renderer.SetHelpText(_defaultHelpText);
                }
                _renderer.EndFrame(); // Restore help text in UI
                return;
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                _isRunning = false;
            }
            // Remove all other movement logic (WASD, arrows, etc.)
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

        // Add MoveShipTo for command logic
        public void MoveShipTo(int x, int y)
        {
            if (_gameState?.PlayerShip != null)
            {
                _gameState.PlayerShip.Position.X = x;
                _gameState.PlayerShip.Position.Y = y;
            }
        }
    }
} 