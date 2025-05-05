using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Core.Models.State;
using SpacePirates.Console.UI.Components;
using System.Runtime.InteropServices;
using System.Text;

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
        private GameViewComponent? _gameComponent;
        private StatusComponent? _statusComponent;
        private CommandComponent? _commandComponent;
        private IGameState? _currentGameState;

        public bool ShowInstructionsPanel { get; set; } = false;

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
            _height = ConsoleConfig.DEFAULT_CONSOLE_HEIGHT;

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
            _statusComponent = new StatusComponent(statusX, 0, statusWidth, ConsoleConfig.MainAreaHeight);
            _gameComponent = new GameViewComponent(gameViewX, 0, ConsoleConfig.GAME_AREA_WIDTH, ConsoleConfig.MainAreaHeight);
            _commandComponent = new CommandComponent(0, ConsoleConfig.MainAreaHeight, _width, ConsoleConfig.HELP_AREA_HEIGHT);

            _isInitialized = true;
            Clear();
        }

        public void BeginFrame()
        {
            if (!_isInitialized) throw new InvalidOperationException("Renderer not initialized");
            
            // Mark all cells as clean for the new frame
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

            // Create buffer writers for each component
            var gameBuffer = new ConsoleBufferWriter(_currentBuffer, _width, _height, (0, 0));
            var statusBuffer = new ConsoleBufferWriter(_currentBuffer, _width, _height, (0, 0));
            var commandBuffer = new ConsoleBufferWriter(_currentBuffer, _width, _height, (0, 0));

            // Render all components
            _gameComponent?.Render(gameBuffer);
            if (ShowInstructionsPanel)
            {
                var instructionsPanel = new InstructionsPanelComponent(_statusComponent.Bounds.X, _statusComponent.Bounds.Y, _statusComponent.Bounds.Width, _statusComponent.Bounds.Height);
                instructionsPanel.Render(statusBuffer);
            }
            else
            {
                _statusComponent?.Render(statusBuffer);
            }
            _commandComponent?.Render(commandBuffer);

            // Draw X axis numbers (1, 12, ..., 75) below the game area
            int numbersY = ConsoleConfig.XAxisLabelRow;
            int gameViewX = _gameComponent != null ? _gameComponent.Bounds.X : 0;
            int xStart = gameViewX + 1; // After left border of game area
            int[] xLabels = { 1, 12, 23, 34, 45, 56, 67, 75 };
            foreach (int x in xLabels)
            {
                int drawX = xStart + x - 1;
                string label = x.ToString();
                for (int j = 0; j < label.Length; j++)
                {
                    _currentBuffer[drawX + j, numbersY].Character = label[j];
                    _currentBuffer[drawX + j, numbersY].Foreground = ConsoleColor.DarkYellow;
                    _currentBuffer[drawX + j, numbersY].IsDirty = true;
                }
            }

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
            _gameComponent?.Update(gameState);
            _statusComponent?.UpdateStatus(gameState);
        }

        public void SetHelpText(string text)
        {
            _commandComponent?.SetHelpText(text);
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
    }
} 