using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Core.Models.Movement;
using SpacePirates.Console.Core.Models.State;
using SpacePirates.API.Models;
using SpacePirates.Console.Game.CommandSystem;
using SpacePirates.Console.Game.Controllers;
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
        public enum ControlState
        {
            StartMenu,
            GalaxyMap,
            SolarSystemMap,
            NameEntry,
            CommandLine
        }
        private readonly IRenderer _renderer;
        private SpacePirates.Console.Core.Models.State.GameState? _gameState;
        private bool _isRunning;
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
        private CommandManager? _commandManager;
        private ShieldController? _shieldController;
        public bool ShowInstructionsPanel => _showInstructionsPanel;
        private ControlState _currentState = ControlState.GalaxyMap;

        public GameEngine(IRenderer renderer)
        {
            _renderer = renderer;
            _isRunning = false;
        }

        public void Initialize()
        {
            _renderer.Initialize();
            _gameState = new SpacePirates.Console.Core.Models.State.GameState();
            _isRunning = true;
            var context = new CommandSystem.CommandContext(this, _renderer);
            _commandManager = new CommandSystem.CommandManager(context, new[] { new CommandSystem.MoveCommand() });
            _renderer.ShowMessage(_defaultHelpText);
            if (_renderer is ConsoleRenderer cr && cr._gameComponent is GameView gpv)
            {
                gpv.SetShipTrail(_shipTrail);
            }
            _shieldController = new ShieldController(_gameState, _renderer);
        }

        public void Initialize(SpacePirates.Console.Core.Models.State.GameState loadedState)
        {
            _renderer.Initialize();
            _gameState = loadedState;
            _isRunning = true;
            var context = new CommandSystem.CommandContext(this, _renderer);
            _commandManager = new CommandSystem.CommandManager(context, new[] { new CommandSystem.MoveCommand() });
            _renderer.ShowMessage(_defaultHelpText);
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
            if (_renderer is ConsoleRenderer cr2 && cr2._gameComponent is GameView gpv2)
            {
                gpv2.SetShipTrail(_shipTrail);
            }
            _shieldController = new ShieldController(_gameState, _renderer);
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
            if (_showQuitConfirm)
            {
                HandleQuitConfirmation(key);
                return;
            }
            switch (_currentState)
            {
                case ControlState.StartMenu:
                    HandleStartMenuInput(key);
                    break;
                case ControlState.GalaxyMap:
                    HandleGalaxyMapInput(key);
                    break;
                case ControlState.SolarSystemMap:
                    HandleSolarSystemInput(key);
                    break;
                case ControlState.NameEntry:
                    HandleNameEntryInput(key);
                    break;
                case ControlState.CommandLine:
                    HandleCommandLineInput(key);
                    break;
            }
        }

        private void HandleQuitConfirmation(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Y)
            {
                _isRunning = false;
            }
            else if (key.Key == ConsoleKey.N)
            {
                _showQuitConfirm = false;
                if (_wasShowingInstructions)
                {
                    _renderer.ShowMessage(_instructionsText);
                }
                else
                {
                    _renderer.ShowMessage(_defaultHelpText);
                }
                if (_renderer is ConsoleRenderer cr1)
                    cr1.ShowInstructionsPanel = _showInstructionsPanel;
                _renderer.EndFrame();
            }
        }

        private void HandleStartMenuInput(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Enter)
            {
                // TODO: Implement menu selection logic
            }
            else if ("hjklHJLK".Contains(key.KeyChar))
            {
                // TODO: Implement menu movement logic
            }
        }

        private void HandleNameEntryInput(ConsoleKeyInfo key)
        {
            // TODO: Implement name entry logic
        }

        private void HandleCommandLineInput(ConsoleKeyInfo key)
        {
            // TODO: Implement command line logic
        }

        private void HandleGalaxyMapInput(ConsoleKeyInfo key)
        {
            if (_renderer is ConsoleRenderer cr && cr._gameComponent is GameView gpv && gpv != null)
            {
                gpv.HandleInput(key);
            }
        }

        private void HandleSolarSystemInput(ConsoleKeyInfo key)
        {
            if (_renderer is ConsoleRenderer cr && cr._gameComponent is GameView gpv && gpv.Map is SolarSystemMapView ssc)
            {
                gpv.HandleInput(key);
            }
            else
            {
                HandleSolarSystemPanelInput(key);
            }
        }

        private void HandleSolarSystemPanelInput(ConsoleKeyInfo key)
        {
            switch (char.ToLower(key.KeyChar))
            {
                case 'm':
                    var moveInput = CommandInputLoop("Move: ");
                    if (!string.IsNullOrWhiteSpace(moveInput))
                    {
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
                            FlyShipTo(x, y);
                        }
                        else
                        {
                            _renderer.ShowMessage("Invalid coordinates. Use: x y (e.g. 12 A or 12A)", true);
                            _renderer.EndFrame();
                            Thread.Sleep(1200);
                            _renderer.ShowMessage(_defaultHelpText);
                        }
                    }
                    else
                    {
                        _renderer.ShowMessage(_defaultHelpText);
                        _renderer.EndFrame();
                    }
                    break;
                case 's':
                    _shieldController?.HandleShieldKey();
                    break;
            }
        }

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

        private void Update()
        {
            if (_gameState == null) return;
            _shieldController?.UpdateShieldCharging();
            _gameState.Update();
            _renderer.UpdateGameState(_gameState);
            if (_renderer is ConsoleRenderer cr)
            {
                var cmdLine = cr.GetCommandLineView();
                cmdLine?.Update();
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

        public void FlyShipTo(int targetX, int targetY)
        {
            if (_gameState?.PlayerShip == null)
                return;
            var movementSystem = new MovementSystem();
            movementSystem.FlyShipTo(
                _gameState.PlayerShip,
                targetX,
                targetY,
                () => {
                    _renderer.UpdateGameState(_gameState);
                    _renderer.BeginFrame();
                    _renderer.EndFrame();
                },
                (msg) => {
                    _renderer.ShowMessage(msg, true);
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
            var exact = _gameState.Galaxy.SolarSystems.Find(s => s.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;
            var endsWith = _gameState.Galaxy.SolarSystems.Find(s =>
                s.Name.Contains("-") &&
                s.Name.Substring(s.Name.LastIndexOf('-') + 1).Equals(input, StringComparison.OrdinalIgnoreCase));
            if (endsWith != null) return endsWith;
            return _gameState.Galaxy.SolarSystems.Find(s => s.Name.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public Galaxy? CurrentGalaxy => _gameState?.Galaxy;
        public ShipTrail? CurrentShipTrail => _shipTrail;
    }
} 