using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Core.Models.State;
using SpacePirates.Console.UI.Components;
using SpacePirates.Console.UI.Views;
using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.UI.Views.Map;
using System.Runtime.InteropServices;
using System.Text;
using SpacePirates.Console.Game.Engine;

namespace SpacePirates.Console.UI.ConsoleRenderer
{
    public class ConsoleRenderer : IRenderer
    {
        private ConsoleBuffer[,] _currentBuffer;
        private ConsoleBuffer[,] _previousBuffer;
        private int _width;
        private int _height;
        private bool _isInitialized;
        private readonly object _consoleLock = new object();

        // UI Components
        internal GameView? _gameComponent;
        private PanelView? _leftPanelComponent;
        private CommandLineView? _commandLineView;
        private IGameState? _currentGameState;

        private SpacePirates.Console.Game.Engine.GameEngine.ControlState _controlState = SpacePirates.Console.Game.Engine.GameEngine.ControlState.GalaxyMap;

        public bool ShowInstructionsPanel { get; set; } = false;

        public static MapView? CurrentMapView { get; private set; }

        private GameControls _controls = new GameControls();
        private StatusPanelStyle _statusStyle = new StatusPanelStyle();
        private InstructionsPanelStyle _instructionsStyle = new InstructionsPanelStyle();

        public ConsoleRenderer()
        {
            _isInitialized = false;
            // Initialize buffers with minimum size that will be resized in Initialize
            _currentBuffer = new ConsoleBuffer[1, 1];
            _previousBuffer = new ConsoleBuffer[1, 1];
            _currentBuffer[0, 0] = new ConsoleBuffer();
            _previousBuffer[0, 0] = new ConsoleBuffer();
        }

        public void Initialize()
        {
            _width = ConsoleConfig.DEFAULT_CONSOLE_WIDTH;
            _height = ConsoleConfig.DEFAULT_CONSOLE_HEIGHT + ConsoleConfig.AXIS_LABEL_HEIGHT;

            // Initialize buffers
            _currentBuffer = new ConsoleBuffer[_width, _height];
            _previousBuffer = new ConsoleBuffer[_width, _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _currentBuffer[x, y] = new ConsoleBuffer();
                    _previousBuffer[x, y] = new ConsoleBuffer();
                }
            }

            // Configure console
            System.Console.Title = "Space Pirates";
            System.Console.CursorVisible = false;
            System.Console.OutputEncoding = Encoding.UTF8;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.Console.WindowWidth = _width;
                System.Console.WindowHeight = _height;
                System.Console.BufferWidth = _width;
                System.Console.BufferHeight = _height;
            }

            // Switch: status panel on the left, game view on the right
            int statusWidth = ConsoleConfig.StatusAreaWidth;
            int statusX = 0;
            int gameViewX = statusWidth; // No extra spacing
            var bounds = (statusX, 0, statusWidth, ConsoleConfig.MainAreaHeight);
            _leftPanelComponent = new ShipStatusView(_controls, _statusStyle, bounds);
            var initialMapView = new GalaxyMapView(null!, (gameViewX, 0, ConsoleConfig.GAME_AREA_WIDTH, ConsoleConfig.MainAreaHeight)); // Will be set with real galaxy in UpdateGameState
            _gameComponent = new GameView(_controls, _statusStyle) { Map = initialMapView, Panel = _leftPanelComponent };
            // Place command area at the very bottom, below axis label row
            int commandY = ConsoleConfig.MainAreaHeight + ConsoleConfig.AXIS_LABEL_HEIGHT;
            _commandLineView = new CommandLineView(_controls, _statusStyle);

            _isInitialized = true;
            Clear();
        }

        public void BeginFrame()
        {
            if (!_isInitialized) throw new InvalidOperationException("Renderer not initialized");
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _currentBuffer[x, y].IsDirty = false;
                }
            }
        }

        public void EndFrame()
        {
            if (!_isInitialized) return;

            var gameBuffer = new ConsoleBufferWriter(_currentBuffer, _width, _height, (0, 0));
            var statusBuffer = new ConsoleBufferWriter(_currentBuffer, _width, _height, (0, 0));
            var commandBuffer = new ConsoleBufferWriter(_currentBuffer, _width, _height, (0, 0));

            // Render all components
            _gameComponent?.Render(gameBuffer);

            // Set CurrentMapView for instructions panel
            CurrentMapView = _gameComponent?.Map;

            var bounds = (0, 0, ConsoleConfig.StatusAreaWidth, ConsoleConfig.MainAreaHeight);
            if (ShowInstructionsPanel)
            {
                _leftPanelComponent = new InstructionsView(_controls, _instructionsStyle, bounds, _controlState);
                _leftPanelComponent.BorderStyle = BoxStyle.Double;
            }
            else if (CurrentMapView is SpacePirates.Console.UI.Views.Map.ISelectableMapView selectable && selectable.SelectedObject != null)
            {
                switch (selectable.SelectedObject)
                {
                    case SpacePirates.API.Models.SolarSystem sys:
                        _leftPanelComponent = new SolarSystemStatusView(_controls, _statusStyle, bounds, sys);
                        _leftPanelComponent.BorderStyle = BoxStyle.Double;
                        break;
                    case SpacePirates.API.Models.Planet planet:
                        if (CurrentMapView is SpacePirates.Console.UI.Views.SolarSystemMapView solarSystemMap)
                            _leftPanelComponent = new PlanetStatusView(_controls, _statusStyle, bounds, planet, solarSystemMap.System);
                        else
                            _leftPanelComponent = new PlanetStatusView(_controls, _statusStyle, bounds, planet, null);
                        _leftPanelComponent.BorderStyle = BoxStyle.Double;
                        break;
                    // Add more cases as needed for other selectable types
                    default:
                        _leftPanelComponent = new ShipStatusView(_controls, _statusStyle, bounds);
                        _leftPanelComponent.BorderStyle = BoxStyle.Double;
                        break;
                }
            }
            else
            {
                _leftPanelComponent = new ShipStatusView(_controls, _statusStyle, bounds);
                _leftPanelComponent.BorderStyle = BoxStyle.Double;
            }
            if (_leftPanelComponent != null && _currentGameState != null)
                _leftPanelComponent.Update(_currentGameState);
            _leftPanelComponent?.Render(statusBuffer);
            _gameComponent.Panel = _leftPanelComponent;
            _commandLineView?.Render(commandBuffer);
        
            // Write changes to console
            var sb = new StringBuilder();
            ConsoleColor currentFg = ConsoleColor.Gray;
            ConsoleColor currentBg = ConsoleColor.Black;

            lock (_consoleLock)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        var current = _currentBuffer[x, y];
                        var previous = _previousBuffer[x, y];

                        if (current.IsDirty || 
                            current.Character != previous.Character ||
                            current.Foreground != previous.Foreground ||
                            current.Background != previous.Background)
                        {
                            // Move cursor
                            sb.Append($"\x1b[{y + 1};{x + 1}H");

                            // Update colors if needed
                            if (current.Foreground != currentFg || current.Background != currentBg)
                            {
                                sb.Append($"\x1b[38;5;{GetColorCode(current.Foreground)}m");
                                sb.Append($"\x1b[48;5;{GetColorCode(current.Background)}m");
                                currentFg = current.Foreground;
                                currentBg = current.Background;
                            }

                            sb.Append(current.Character);

                            // Copy to previous buffer
                            previous.Character = current.Character;
                            previous.Foreground = current.Foreground;
                            previous.Background = current.Background;
                        }
                    }
                }

                // Reset colors
                sb.Append("\x1b[0m");
                System.Console.Write(sb.ToString());
            }
        }

        public void Clear()
        {
            if (!_isInitialized) return;

            lock (_consoleLock)
            {
                System.Console.Write("\x1b[2J\x1b[H");
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        _currentBuffer[x, y].Character = ' ';
                        _currentBuffer[x, y].Foreground = ConsoleColor.Gray;
                        _currentBuffer[x, y].Background = ConsoleColor.Black;
                        _currentBuffer[x, y].IsDirty = true;
                    }
                }
            }
        }

        public void HandleInput(ConsoleKeyInfo keyInfo)
        {
            _gameComponent?.HandleInput(keyInfo);
        }

        public void UpdateGameState(IGameState gameState)
        {
            _currentGameState = gameState;
            _gameComponent?.Map?.Update(gameState);
            _leftPanelComponent?.Update(gameState);
        }

        public void SetHelpText(string text)
        {
            if (_commandLineView != null) _commandLineView.SetHelpText(text);
        }

        public void ShowMessage(string message, bool temporary = false)
        {
            _commandLineView?.ShowMessage(message, temporary);
        }

        public void SetControlState(SpacePirates.Console.Game.Engine.GameEngine.ControlState state)
        {
            _controlState = state;
        }

        public void SetTemporaryNotification(string message)
        {
            _commandLineView?.SetTemporaryMessage(message);
        }

        private static int GetColorCode(ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Black => 0,
                ConsoleColor.DarkBlue => 4,
                ConsoleColor.DarkGreen => 2,
                ConsoleColor.DarkCyan => 6,
                ConsoleColor.DarkRed => 1,
                ConsoleColor.DarkMagenta => 5,
                ConsoleColor.DarkYellow => 3,
                ConsoleColor.Gray => 7,
                ConsoleColor.DarkGray => 8,
                ConsoleColor.Blue => 12,
                ConsoleColor.Green => 10,
                ConsoleColor.Cyan => 14,
                ConsoleColor.Red => 9,
                ConsoleColor.Magenta => 13,
                ConsoleColor.Yellow => 11,
                ConsoleColor.White => 15,
                _ => 7
            };
        }

        public void Dispose()
        {
            if (_isInitialized)
            {
                System.Console.CursorVisible = true;
                System.Console.ResetColor();
                System.Console.Clear();
            }
        }

        public CommandLineView? GetCommandLineView()
        {
            return _commandLineView;
        }
    }
} 