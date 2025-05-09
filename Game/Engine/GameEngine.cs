using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Core.Models.Movement;
using SpacePirates.Console.Core.Models.State;
using SpacePirates.API.Models;
using SpacePirates.Console.UI.InputHandling.CommandSystem;
using SpacePirates.Console.UI.ConsoleRenderer;
using System.Text;
using System.ComponentModel;
using System.Linq;
using SpacePirates.Console.UI.Views;
using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Views.Map;

namespace SpacePirates.Console.Game.Engine
{
    public class GameEngine
    {
        private readonly IRenderer _renderer;
        private SpacePirates.Console.Core.Models.State.GameState? _gameState;
        private bool _isRunning;
        private CommandParser? _commandParser;
        private MoveCommand? _moveCommand;
        private string _defaultHelpText = "Tab to toggle instructions | ESC to exit";
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
        private DateTime _lastHelpTextUpdate = DateTime.MinValue;
        private double _helpTextTimeoutSeconds = 2.5;
        private bool _helpTextIsTemporary = false;
        public bool ShowInstructionsPanel => _showInstructionsPanel;

        public enum ControlState
        {
            StartMenu,
            GalaxyMap,
            SolarSystemView,
            NameEntry,
            CommandLine
        }
        private ControlState _currentState = ControlState.GalaxyMap;

        public GameEngine(IRenderer renderer)
        {
            _renderer = renderer;
            _isRunning = false;
        }

        public void Initialize()
        {
            _renderer.Initialize();  // Use ConsoleConfig values instead of hardcoded dimensions
            _gameState = new SpacePirates.Console.Core.Models.State.GameState(); // Initialize with default state
            _isRunning = true;

            // Setup command system
            var context = new CommandContext(this, _renderer);
            _moveCommand = new MoveCommand();
            _commandParser = new CommandParser(context, new[] { _moveCommand });

            // Set initial help text
            _renderer.SetHelpText(_defaultHelpText);

            // Pass trail to GameViewComponent if possible
            if (_renderer is ConsoleRenderer cr && cr._gameComponent is GameView gpv)
            {
                gpv.SetShipTrail(_shipTrail);
            }
        }

        public void Initialize(SpacePirates.Console.Core.Models.State.GameState loadedState)
        {
            _renderer.Initialize();
            _gameState = loadedState;
            _isRunning = true;

            // Setup command system
            var context = new CommandContext(this, _renderer);
            _moveCommand = new MoveCommand();
            _commandParser = new CommandParser(context, new[] { _moveCommand });

            // Set initial help text
            _renderer.SetHelpText(_defaultHelpText);

            // Show the galaxy map as the main component
            if (_renderer is ConsoleRenderer cr && _gameState is GameState gs && gs.Galaxy != null)
            {
                if (cr._gameComponent is GameView gpv)
                {
                    int gameViewX = ConsoleConfig.StatusAreaWidth;
                    var bounds = (gameViewX, 0, ConsoleConfig.GAME_AREA_WIDTH, ConsoleConfig.MainAreaHeight);
                    gpv.SetMapView(new GalaxyMapView(gs.Galaxy, bounds));
                    gpv.Controls = new GalaxyControls();
                }
            }

            // Pass trail to GameViewComponent if possible
            if (_renderer is ConsoleRenderer cr2 && cr2._gameComponent is GameView gpv2)
            {
                gpv2.SetShipTrail(_shipTrail);
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

            switch (_currentState)
            {
                case ControlState.StartMenu:
                    // Only allow h/j/k/l and Enter for menu navigation
                    if (key.Key == ConsoleKey.Enter)
                    {
                        // TODO: Implement menu selection logic
                    }
                    else if ("hjklHJLK".Contains(key.KeyChar))
                    {
                        // TODO: Implement menu movement logic
                    }
                    break;
                case ControlState.GalaxyMap:
                    HandleGalaxyMapInput(key);
                    break;
                case ControlState.SolarSystemView:
                    HandleSolarSystemInput(key);
                    break;
                case ControlState.NameEntry:
                    // Only allow text input and Enter
                    break;
                case ControlState.CommandLine:
                    // Only allow command line input
                    break;
            }
        }

        private void HandleGalaxyMapInput(ConsoleKeyInfo key)
        {
            if (_renderer is ConsoleRenderer cr && cr._gameComponent is GameView gpv && gpv != null)
            {
                gpv.HandleInput(key);
            }
        }

        // Reusable command mode input loop
        private string CommandInputLoop(string prompt)
        {
            _isCommandMode = true;
            _commandInput = string.Empty;
            _showCursor = true;
            _lastCursorBlink = DateTime.Now;
            _renderer.SetHelpText(prompt);
            _renderer.EndFrame();
            string input = string.Empty;
            while (_isCommandMode)
            {
                if ((DateTime.Now - _lastCursorBlink) > _cursorBlinkInterval)
                {
                    _showCursor = !_showCursor;
                    _lastCursorBlink = DateTime.Now;
                    _renderer.SetHelpText(prompt + input + (_showCursor ? "_" : " "));
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
                        input = string.Empty;
                        break;
                    }
                    if (cmdKey.Key == ConsoleKey.Backspace && input.Length > 0)
                    {
                        input = input.Substring(0, input.Length - 1);
                    }
                    else if (!char.IsControl(cmdKey.KeyChar))
                    {
                        input += cmdKey.KeyChar;
                    }
                    _renderer.SetHelpText(prompt + input + (_showCursor ? "_" : " "));
                    _renderer.EndFrame();
                }
                Thread.Sleep(16);
            }
            return input;
        }

        private void HandleSolarSystemInput(ConsoleKeyInfo key)
        {
            if (_renderer is ConsoleRenderer cr && cr._gameComponent is GameView gpv && gpv.Map is SolarSystemMapView ssc)
            {
                gpv.HandleInput(key);
            }
            else
            {
                // Fallback to old logic if not using SolarSystemComponent
                // Only allow m, s, Tab, Esc
                switch (char.ToLower(key.KeyChar))
                {
                    case 'm':
                        // Start move command
                        var moveInput = CommandInputLoop("Move: ");
                        if (!string.IsNullOrWhiteSpace(moveInput))
                        {
                            // Accept formats like '12 A', '12A', '12,a', '12a', etc.
                            moveInput = moveInput.Replace(",", " ").Replace("-", " ").Trim();
                            string xPart = string.Empty, yPart = string.Empty;
                            var parts = moveInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 2)
                            {
                                xPart = parts[0];
                                yPart = parts[1];
                            }
                            else if (parts.Length == 1)
                            {
                                // Try to split number and letter, e.g. 12A
                                var s = parts[0];
                                int i = 0;
                                while (i < s.Length && char.IsDigit(s[i])) i++;
                                if (i > 0 && i < s.Length)
                                {
                                    xPart = s.Substring(0, i);
                                    yPart = s.Substring(i);
                                }
                            }
                            if (int.TryParse(xPart, out int x) && yPart.Length == 1 && char.IsLetter(yPart[0]))
                            {
                                int y = char.ToUpper(yPart[0]) - 'A' + 1;
                                MoveShipTo(x, y);
                            }
                            else
                            {
                                _renderer.SetHelpText("Invalid coordinates. Use: x y (e.g. 12 A or 12A)");
                                _renderer.EndFrame();
                                Thread.Sleep(1200);
                                _renderer.SetHelpText(_defaultHelpText);
                            }
                        }
                        else
                        {
                            _renderer.SetHelpText(_defaultHelpText);
                            _renderer.EndFrame();
                        }
                        break;
                    case 's':
                        // Toggle shield with charging animation
                        if (_gameState?.PlayerShip?.Shield != null)
                        {
                            var shield = _gameState.PlayerShip.Shield;
                            if (!shield.IsActive && !shield.Charging)
                            {
                                shield.Charging = true;
                                shield.CurrentIntegrity = 0;
                                _renderer.SetHelpText("Shield charging...");
                                _renderer.EndFrame();
                                int maxCapacity = shield.CalculateMaxCapacity();
                                int chargeSteps = 20;
                                int msPerStep = 10000 / chargeSteps;
                                for (int i = 1; i <= chargeSteps; i++)
                                {
                                    shield.CurrentIntegrity = (int)(maxCapacity * (i / (double)chargeSteps));
                                    _renderer.SetHelpText($"Shield charging... {shield.CurrentIntegrity * 100 / maxCapacity}%");
                                    _renderer.EndFrame();
                                    Thread.Sleep(msPerStep);
                                }
                                shield.CurrentIntegrity = maxCapacity;
                                shield.IsActive = true;
                                shield.Charging = false;
                                _renderer.SetHelpText("Shield fully charged!");
                                _renderer.EndFrame();
                                Thread.Sleep(800);
                                _renderer.SetHelpText(_defaultHelpText);
                            }
                            else if (shield.IsActive && !shield.Charging)
                            {
                                shield.IsActive = false;
                                shield.CurrentIntegrity = 0;
                                _renderer.SetHelpText("Shield deactivated!");
                                _renderer.EndFrame();
                                Thread.Sleep(800);
                                _renderer.SetHelpText(_defaultHelpText);
                            }
                        }
                        break;
                }
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
                    SetTemporaryHelpText("Shield fully charged!");
                }
            }

            // Update game state
            _gameState.Update();
            
            // Update renderer with new state
            _renderer.UpdateGameState(_gameState);

            // Update the command line view for temporary message timeout
            if (_renderer is ConsoleRenderer cr)
            {
                var cmdLine = cr.GetCommandLineView();
                cmdLine?.Update();
            }

            // Restore default help text if a temporary message was set and timeout expired
            if (_helpTextIsTemporary && !_isCommandMode)
            {
                if ((DateTime.UtcNow - _lastHelpTextUpdate).TotalSeconds > _helpTextTimeoutSeconds)
                {
                    _renderer.SetHelpText(_defaultHelpText);
                    _helpTextIsTemporary = false;
                }
            }
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

        // Utility to set help text with optional timeout
        private void SetTemporaryHelpText(string text, double timeoutSeconds = 2.5)
        {
            _renderer.SetHelpText(text);
            _lastHelpTextUpdate = DateTime.UtcNow;
            _helpTextTimeoutSeconds = timeoutSeconds;
            _helpTextIsTemporary = true;
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
                    SetTemporaryHelpText(msg);
                    _renderer.EndFrame();
                },
                _shipTrail
            );
        }

        public SolarSystem? FindSolarSystem(string input)
        {
            if (_gameState?.Galaxy == null) return null;
            if (int.TryParse(input, out int id))
                return _gameState.Galaxy.SolarSystems.Find(s => s.Id == id);

            // Try exact match
            var exact = _gameState.Galaxy.SolarSystems.Find(s => s.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;

            // Try ends-with match for hex suffix after dash
            var endsWith = _gameState.Galaxy.SolarSystems.Find(s =>
                s.Name.Contains("-") &&
                s.Name.Substring(s.Name.LastIndexOf('-') + 1).Equals(input, StringComparison.OrdinalIgnoreCase));
            if (endsWith != null) return endsWith;

            // Try contains match
            return _gameState.Galaxy.SolarSystems.Find(s => s.Name.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public Galaxy? CurrentGalaxy => _gameState?.Galaxy;
        public ShipTrail? CurrentShipTrail => _shipTrail;
    }
} 