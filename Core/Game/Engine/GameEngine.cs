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
        private readonly MovementSystem _movementSystem = new MovementSystem();
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
            if (_renderer is ConsoleRenderer cr && cr._gameComponent is GameView gpv && gpv != null)
            {
                gpv.HandleInput(key);
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

        public void FlyShipTo(int targetX, int targetY)
        {
            if (_gameState?.PlayerShip == null)
                return;
            _movementSystem.FlyShipTo(
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
            // After movement, persist position and fuel
            var api = AppDomain.CurrentDomain.GetData("ApiClient") as SpacePirates.Console.UI.Components.ApiClient;
            var ship = _gameState.PlayerShip;
            if (api != null && ship != null)
            {
                var dto = new {
                    Position = new { X = ship.Position.X, Y = ship.Position.Y },
                    FuelSystem = new { CurrentLevel = ship.FuelSystem.CurrentLevel, CurrentFuel = ship.FuelSystem.CurrentFuel }
                };
                _ = api.UpdateShipStateAsync(ship.Id, dto);
            }
        }

        public Galaxy? CurrentGalaxy => _gameState?.Galaxy;
        public ShipTrail? CurrentShipTrail => _shipTrail;
    }
} 