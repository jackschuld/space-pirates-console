using System;

namespace SpacePirates.Console.Core.Models.State
{
    public static class ConsoleConfig
    {
        // Console dimensions
        public const int DEFAULT_CONSOLE_WIDTH = 120;  // Standard wide console
        public const int DEFAULT_CONSOLE_HEIGHT = 30;  // Standard height

        // Layout configuration
        public const int GAME_AREA_WIDTH = 80;  // Main game view width
        public const int HELP_AREA_HEIGHT = 3;  // Height of help section at bottom
        
        // Calculated values
        public static int StatusAreaWidth => DEFAULT_CONSOLE_WIDTH - GAME_AREA_WIDTH - 1;  // -1 for separator
        public static int MainAreaHeight => DEFAULT_CONSOLE_HEIGHT - HELP_AREA_HEIGHT;
        
        // Usable game area (accounting for borders)
        public static int UsableGameWidth => GAME_AREA_WIDTH - 2;  // -2 for left and right borders
        public static int UsableGameHeight => MainAreaHeight - 2;  // -2 for top and bottom borders
    }
} 