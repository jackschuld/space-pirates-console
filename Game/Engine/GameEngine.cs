using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Core.Models.Movement;
using SpacePirates.Console.Core.Models.State;
using SpacePirates.API.Models;
using SpacePirates.Console.UI.InputHandling.CommandSystem;
using SpacePirates.Console.UI.ConsoleRenderer;
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
        private string _defaultHelpText = "Type 'c' to enter command mode | ESC to exit";
        private string _lastCommandResult = string.Empty;
        private bool _showInstructions = false;
        private readonly string _instructionsText =
            "COMMANDS:\n" +
            MoveCommand.Description + "\n" +
            "c esc - Quit game\n" +
            "Tab - Show/hide instructions\n";
        private bool _showQuitConfirm = false;
        private bool _wasShowingInstructions = false;
        private bool _showInstructionsPanel = false;
        private bool _isCommandMode = false;
        private string _commandInput = string.Empty;
        private bool _showCursor = true;
        private DateTime _lastCursorBlink = DateTime.Now;
        private readonly TimeSpan _cursorBlinkInterval = TimeSpan.FromMilliseconds(500);
        private ShipTrail _shipTrail = new ShipTrail(12);
        public bool ShowInstructionsPanel => _showInstructionsPanel;

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

            // Pass trail to GameViewComponent if possible
            if (_renderer is ConsoleRenderer cr && cr._gameComponent != null)
            {
                cr._gameComponent.ShipTrail = _shipTrail;
            }
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

            // If showing quit confirmation, only accept y/n
            if (_showQuitConfirm)
            {
                if (key.Key == ConsoleKey.Y)
                {
                    _isRunning = false;
                }
                else if (key.Key == ConsoleKey.N)
                {
                    _showQuitConfirm = false;
                    // Restore previous screen (instructions or normal)
                    if (_wasShowingInstructions)
                    {
                        _renderer.SetHelpText(_instructionsText);
                    }
                    else
                    {
                        _renderer.SetHelpText(_defaultHelpText);
                    }
                    if (_renderer is ConsoleRenderer cr1)
                        cr1.ShowInstructionsPanel = _showInstructionsPanel;
                    _renderer.EndFrame();
                }
                // Ignore all other input while confirmation is shown
                return;
            }

            // Quick key: ESC to quit (show confirmation)
            if (key.Key == ConsoleKey.Escape)
            {
                _showQuitConfirm = true;
                _wasShowingInstructions = _showInstructions;
                _renderer.SetHelpText("Are you sure you want to quit? (y/n)");
                _renderer.EndFrame();
                return;
            }

            // Quick key: Tab to toggle instructions panel
            if (key.Key == ConsoleKey.Tab)
            {
                _showInstructionsPanel = !_showInstructionsPanel;
                if (_renderer is ConsoleRenderer cr2)
                    cr2.ShowInstructionsPanel = _showInstructionsPanel;
                _renderer.EndFrame();
                return;
            }

            if (_showInstructions)
            {
                // Ignore all other input while instructions are shown
                return;
            }

            if (key.KeyChar == 'c' || key.KeyChar == 'C')
            {
                _isCommandMode = true;
                _commandInput = string.Empty;
                _showCursor = true;
                _lastCursorBlink = DateTime.Now;
                _renderer.SetHelpText(""); // Clear help text for command entry
                _renderer.EndFrame(); // Force UI update

                while (_isCommandMode)
                {
                    // Blinking cursor logic
                    if ((DateTime.Now - _lastCursorBlink) > _cursorBlinkInterval)
                    {
                        _showCursor = !_showCursor;
                        _lastCursorBlink = DateTime.Now;
                        _renderer.SetHelpText(_commandInput + (_showCursor ? "_" : " "));
                        _renderer.EndFrame();
                    }

                    if (System.Console.KeyAvailable)
                    {
                        var cmdKey = System.Console.ReadKey(true);
                        if (cmdKey.Key == ConsoleKey.Enter)
                        {
                            _isCommandMode = false;
                            break;
                        }
                        if (cmdKey.Key == ConsoleKey.Escape)
                        {
                            _isCommandMode = false;
                            _commandInput = string.Empty;
                            break;
                        }
                        if (cmdKey.Key == ConsoleKey.Backspace && _commandInput.Length > 0)
                        {
                            _commandInput = _commandInput.Substring(0, _commandInput.Length - 1);
                        }
                        else if (!char.IsControl(cmdKey.KeyChar))
                        {
                            _commandInput += cmdKey.KeyChar;
                        }
                        _renderer.SetHelpText(_commandInput + (_showCursor ? "_" : " "));
                        _renderer.EndFrame();
                    }
                    Thread.Sleep(16); // ~60 FPS
                }

                var commandInput = _commandInput;
                _commandInput = string.Empty;
                var result = _commandParser?.ParseAndExecuteWithResult(commandInput);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    _renderer.SetHelpText(result);
                    _renderer.EndFrame();
                    // Wait 2.5 seconds, then restore default help text
                    var restoreTime = DateTime.Now + TimeSpan.FromSeconds(2.5);
                    while (DateTime.Now < restoreTime)
                    {
                        Thread.Sleep(50);
                        // If a key is pressed, break early to avoid blocking input
                        if (System.Console.KeyAvailable) break;
                    }
                    _renderer.SetHelpText(_defaultHelpText);
                }
                else
                {
                    _renderer.SetHelpText(_defaultHelpText);
                }
                if (_renderer is ConsoleRenderer cr3)
                    cr3.ShowInstructionsPanel = _showInstructionsPanel;
                _renderer.EndFrame(); // Restore help text in UI
                return;
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

        // Add MoveShipTo for command logic
        public void MoveShipTo(int targetX, int targetY)
        {
            if (_gameState?.PlayerShip == null)
                return;

            var movementSystem = new MovementSystem();
            movementSystem.MoveShipTo(
                _gameState.PlayerShip,
                targetX,
                targetY,
                () => {
                    _renderer.UpdateGameState(_gameState);
                    _renderer.BeginFrame();
                    _renderer.EndFrame();
                },
                (msg) => {
                    _renderer.SetHelpText(msg);
                    _renderer.EndFrame();
                },
                _shipTrail
            );
        }
    }
} 