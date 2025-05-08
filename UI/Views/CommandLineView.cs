using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.Core.Interfaces;
using System;

namespace SpacePirates.Console.UI.Views
{
    public class CommandLineView : BaseView
    {
        public enum CommandBarMode { Help, CommandInput, TemporaryMessage }

        private string _helpText = string.Empty;
        private string _commandInput = string.Empty;
        private string _temporaryMessage = string.Empty;
        private CommandBarMode _mode = CommandBarMode.Help;
        private bool _showCursor = true;

        public CommandLineView(BaseControls controls, BaseStyleProvider styleProvider)
        {
            Controls = controls;
            StyleProvider = styleProvider;
        }

        public void SetHelpText(string text)
        {
            _helpText = text;
            _mode = CommandBarMode.Help;
        }

        public void SetCommandInput(string input, bool showCursor = true)
        {
            _commandInput = input;
            _showCursor = showCursor;
            _mode = CommandBarMode.CommandInput;
        }

        public void SetTemporaryMessage(string message)
        {
            _temporaryMessage = message;
            _mode = CommandBarMode.TemporaryMessage;
        }

        public void SetMode(CommandBarMode mode)
        {
            _mode = mode;
        }

        public override void Render()
        {
            // No-op: only buffer-based rendering is used
        }

        public override void HandleInput(ConsoleKeyInfo key)
        {
            Controls.HandleInput(key, this);
        }

        public void Render(IBufferWriter buffer)
        {
            int width = SpacePirates.Console.Core.Models.State.ConsoleConfig.DEFAULT_CONSOLE_WIDTH;
            int y = SpacePirates.Console.Core.Models.State.ConsoleConfig.DEFAULT_CONSOLE_HEIGHT - 1;

            // Draw background bar
            for (int x = 0; x < width; x++)
            {
                buffer.DrawChar(x, y, ' ', null, ConsoleColor.Black);
            }

            // Draw prompt
            int promptX = 2;
            buffer.DrawString(promptX, y, "> ", PanelStyles.CommandPromptColor, ConsoleColor.Black);

            // Draw content based on mode
            int textX = promptX + 2;
            switch (_mode)
            {
                case CommandBarMode.Help:
                    if (!string.IsNullOrEmpty(_helpText))
                        buffer.DrawString(textX, y, _helpText, PanelStyles.CommandTextColor, ConsoleColor.Black);
                    break;
                case CommandBarMode.CommandInput:
                    string input = _commandInput;
                    if (_showCursor)
                        input += "_";
                    buffer.DrawString(textX, y, input, PanelStyles.CommandTextColor, ConsoleColor.Black);
                    break;
                case CommandBarMode.TemporaryMessage:
                    if (!string.IsNullOrEmpty(_temporaryMessage))
                        buffer.DrawString(textX, y, _temporaryMessage, PanelStyles.CommandTextColor, ConsoleColor.Black);
                    break;
            }
        }
    }
} 