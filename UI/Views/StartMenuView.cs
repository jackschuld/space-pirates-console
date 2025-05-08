using System;
using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;

namespace SpacePirates.Console.UI.Views
{
    public class StartMenuView : BaseView
    {
        public enum MenuOption { StartNewGame, LoadGame, Quit }
        public MenuOption SelectedOption { get; private set; } = MenuOption.StartNewGame;
        private int _selectedIndex = 0;
        private readonly string[] _options = { "Start New Game", "Load Saved Game", "Quit" };
        private readonly bool _loadEnabled;

        public StartMenuView(bool loadEnabled = true)
        {
            Controls = new MenuControls();
            StyleProvider = null; // Optionally add a style provider for menu
            _loadEnabled = loadEnabled;
        }

        public void Show()
        {
            ConsoleKeyInfo key;
            do
            {
                Render();
                key = System.Console.ReadKey(true);
                HandleInput(key);
            } while (key.Key != ConsoleKey.Enter || (!_loadEnabled && _selectedIndex == 1));
            SelectedOption = (MenuOption)_selectedIndex;
        }

        public override void Render()
        {
            System.Console.Clear();
            System.Console.WriteLine("============================");
            System.Console.WriteLine("   SPACE PIRATES - MAIN MENU");
            System.Console.WriteLine("============================\n");
            for (int i = 0; i < _options.Length; i++)
            {
                if (i == 1 && !_loadEnabled)
                {
                    System.Console.ForegroundColor = ConsoleColor.DarkGray;
                    System.Console.WriteLine($"  {_options[i]} (no saves)");
                    System.Console.ResetColor();
                }
                else if (i == _selectedIndex)
                {
                    System.Console.ForegroundColor = ConsoleColor.Cyan;
                    System.Console.WriteLine($"> {_options[i]}");
                    System.Console.ResetColor();
                }
                else
                {
                    System.Console.WriteLine($"  {_options[i]}");
                }
            }
            System.Console.WriteLine("\nUse j,k to move up and down, and Enter to select.");
        }

        public override void HandleInput(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.UpArrow || key.KeyChar == 'k' || key.KeyChar == 'K')
            {
                do {
                    _selectedIndex = (_selectedIndex - 1 + _options.Length) % _options.Length;
                } while (_selectedIndex == 1 && !_loadEnabled);
            }
            else if (key.Key == ConsoleKey.DownArrow || key.KeyChar == 'j' || key.KeyChar == 'J')
            {
                do {
                    _selectedIndex = (_selectedIndex + 1) % _options.Length;
                } while (_selectedIndex == 1 && !_loadEnabled);
            }
        }
    }
} 