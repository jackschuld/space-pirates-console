using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Core.Models.Movement;
using SpacePirates.Console.Core.Models.State;
using SpacePirates.API.Models;
using SpacePirates.Console.UI.InputHandling.CommandSystem;
using SpacePirates.Console.UI.ConsoleRenderer;
using System.Text;
using SpacePirates.Console.Core.Models.Galaxy;
using SpacePirates.Console.UI.Components;

namespace SpacePirates.Console.Game.Engine
{
    public class GameEngine
    {
        private readonly IRenderer _renderer;
        private IGameState? _gameState;
        private bool _isRunning;
        private CommandParser? _commandParser;
        private MoveCommand? _moveCommand;
        private string _defaultHelpText = SpacePirates.Console.UI.Components.CommandComponent.DefaultHelpText;
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
        private DateTime _lastShieldChargeUpdate = DateTime.UtcNow;
        private double _shieldChargeProgress = 0;
        public bool ShowInstructionsPanel => _showInstructionsPanel;

        // Galaxy map integration
        private GameMode _gameMode = GameMode.GalaxyMap;
        private Galaxy? _galaxy;
        private GalaxyMapState? _galaxyMapState;
        private GalaxyMapComponent? _galaxyMapComponent;

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
            if (_renderer is ConsoleRenderer cr && cr._gameComponent is GameViewComponent gvc)
            {
                gvc.ShipTrail = _shipTrail;
            }

            // Initialize galaxy and map state
            int galaxySeed = DateTime.UtcNow.Millisecond + DateTime.UtcNow.Second * 1000;
            _galaxy = new Galaxy(galaxySeed);
            _galaxyMapState = new GalaxyMapState();
            int statusWidth = ConsoleConfig.StatusAreaWidth;
            int gameViewX = statusWidth;
            _galaxyMapComponent = new GalaxyMapComponent(
                gameViewX, 0, ConsoleConfig.GAME_AREA_WIDTH, ConsoleConfig.MainAreaHeight,
                _galaxy, _galaxyMapState,
                onInspect: msg => _renderer.SetHelpText(msg),
                onLightspeedPrompt: msg => _renderer.SetHelpText(msg)
            );
            _gameMode = GameMode.GalaxyMap;
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

            // Route input to galaxy map if in galaxy mode
            if (_gameMode == GameMode.GalaxyMap && _galaxyMapComponent != null)
            {
                _galaxyMapComponent.HandleInput(key);
                // Example: handle lightspeed confirmation (not full implementation)
                if (key.Key == ConsoleKey.Y && _renderer != null)
                {
                    // Confirm lightspeed: switch to solar system mode
                    _gameMode = GameMode.SolarSystem;
                    _renderer.SetHelpText("Entering solar system...");
                }
                if (key.Key == ConsoleKey.N && _renderer != null)
                {
                    _renderer.SetHelpText(_defaultHelpText);
                }
                return;
            }

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

            // If a command key is pressed, enter command mode with prefix
            if (char.IsLetter(key.KeyChar))
            {
                string prefix = string.Empty;
                switch (char.ToLower(key.KeyChar))
                {
                    case 'm':
                        prefix = "Move: ";
                        break;
                    // Add more cases for other commands as needed
                }
                if (!string.IsNullOrEmpty(prefix))
                {
                    _isCommandMode = true;
                    _commandInput = string.Empty;
                    _showCursor = true;
                    _lastCursorBlink = DateTime.Now;
                    _renderer.SetHelpText(prefix);
                    _renderer.EndFrame();

                    while (_isCommandMode)
                    {
                        if ((DateTime.Now - _lastCursorBlink) > _cursorBlinkInterval)
                        {
                            _showCursor = !_showCursor;
                            _lastCursorBlink = DateTime.Now;
                            _renderer.SetHelpText(prefix + _commandInput + (_showCursor ? "_" : " "));
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
                            _renderer.SetHelpText(prefix + _commandInput + (_showCursor ? "_" : " "));
                            _renderer.EndFrame();
                        }
                        Thread.Sleep(16);
                    }
                    // Remove the prefix and build the actual command string
                    string commandInput = string.Empty;
                    switch (prefix)
                    {
                        case "Move: ":
                            commandInput = "move " + _commandInput;
                            break;
                        // Add more cases for other commands as needed
                    }
                    _commandInput = string.Empty;
                    var result = _commandParser?.ParseAndExecuteWithResult(commandInput);
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        _renderer.SetHelpText(result);
                        _renderer.EndFrame();
                        var restoreTime = DateTime.Now + TimeSpan.FromSeconds(2.5);
                        while (DateTime.Now < restoreTime)
                        {
                            Thread.Sleep(50);
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
                    _renderer.EndFrame();
                    return;
                }
            }

            // Shield activation: 's' to start charging if off, or turn off if on
            if (key.Key == ConsoleKey.S && _gameState?.PlayerShip?.Shield != null)
            {
                var shield = _gameState.PlayerShip.Shield;
                if (!shield.IsActive && !shield.Charging)
                {
                    shield.Charging = true;
                    shield.CurrentIntegrity = 0;
                    _lastShieldChargeUpdate = DateTime.UtcNow;
                    _shieldChargeProgress = 0;
                    _renderer.SetHelpText("Shield charging...");
                }
                else if (shield.IsActive && !shield.Charging)
                {
                    shield.IsActive = false;
                    shield.CurrentIntegrity = 0;
                    _renderer.SetHelpText("Turning shields off...");
                    // Hide message after 2.5s
                    var restoreTime = DateTime.Now + TimeSpan.FromSeconds(2.5);
                    while (DateTime.Now < restoreTime)
                    {
                        Thread.Sleep(50);
                        if (System.Console.KeyAvailable) break;
                    }
                    _renderer.SetHelpText(_defaultHelpText);
                }
                return;
            }
        }

        private void Update()
        {
            if (_gameState == null) return;

            // Shield charging logic
            var ship = _gameState.PlayerShip;
            if (ship?.Shield != null && ship.Shield.Charging)
            {
                var now = DateTime.UtcNow;
                double seconds = (now - _lastShieldChargeUpdate).TotalSeconds;
                _lastShieldChargeUpdate = now;
                // Charge up shield: 100% in 15 seconds, 1% per 0.15s
                int maxCapacity = ship.Shield.CalculateMaxCapacity();
                double percentPerSecond = 100.0 / 15.0;
                _shieldChargeProgress += percentPerSecond * seconds;
                int newPercent = (int)_shieldChargeProgress;
                newPercent = Math.Clamp(newPercent, 0, 100);
                ship.Shield.CurrentIntegrity = (int)(maxCapacity * (newPercent / 100.0));
                if (newPercent >= 100)
                {
                    ship.Shield.CurrentIntegrity = maxCapacity;
                    ship.Shield.IsActive = true;
                    ship.Shield.Charging = false;
                    _renderer.SetHelpText("Shield fully charged!");
                    // Hide message after 2.5s
                    var restoreTime = DateTime.Now + TimeSpan.FromSeconds(2.5);
                    while (DateTime.Now < restoreTime)
                    {
                        Thread.Sleep(50);
                        if (System.Console.KeyAvailable) break;
                    }
                    _renderer.SetHelpText(_defaultHelpText);
                }
            }

            // Update game state
            _gameState.Update();
            
            // Update renderer with new state
            _renderer.UpdateGameState(_gameState);
        }

        private void Render()
        {
            _renderer.BeginFrame();
            // Render galaxy map if in galaxy mode
            if (_gameMode == GameMode.GalaxyMap && _galaxyMapComponent != null && _renderer is ConsoleRenderer cr)
            {
                cr._gameComponent = _galaxyMapComponent;
            }
            // (Else, use the normal game view component)
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