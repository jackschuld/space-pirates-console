using System;

namespace SpacePirates.Console.Core.Models.State
{
    public static class ConsoleConfig
    {
        // Console dimensions
        public const int DEFAULT_CONSOLE_WIDTH = 120;  // Standard wide console
        public const int DEFAULT_CONSOLE_HEIGHT = 30;  // Standard height

        // Layout configuration
        public const int GAME_AREA_WIDTH = 76;  // Main game view width for 1-75
        public const int HELP_AREA_HEIGHT = 2;  // Height of help section at bottom
        public const int AXIS_LABEL_HEIGHT = 1; // Height for axis labels below the map
        
        // Calculated values
        public static int StatusAreaWidth => DEFAULT_CONSOLE_WIDTH - GAME_AREA_WIDTH - 2;
        public static int MainAreaHeight => DEFAULT_CONSOLE_HEIGHT - HELP_AREA_HEIGHT - AXIS_LABEL_HEIGHT + 1;
        
        // Usable game area (accounting for borders)
        public static int UsableGameWidth => GAME_AREA_WIDTH - 2;  // -2 for left and right borders (1-75)
        public static int UsableGameHeight => MainAreaHeight - 2;  // -2 for top and bottom borders

        public static int XAxisLabelRow => MainAreaHeight; // Row just below the game area
    }
} 