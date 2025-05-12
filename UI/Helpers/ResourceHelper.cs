using System;

namespace SpacePirates.Console.UI.Helpers
{
    public static class ResourceHelper
    {
        public static string GetResourceName(string resourceName)
        {
            resourceName = resourceName.ToLowerInvariant();
            if (resourceName.Contains("fuel ore")) return "Deuterium Ore";
            if (resourceName.Contains("hull alloy")) return "Titanium Alloy";
            if (resourceName.Contains("weapon crystal")) return "Iridium Crystal";
            if (resourceName.Contains("shield plasma")) return "Helium-3 Plasma";
            if (resourceName.Contains("engine parts")) return "Graphene Matrix";
            return resourceName.Length > 0 ? char.ToUpper(resourceName[0]) + resourceName.Substring(1) : resourceName;
        }

        public static ConsoleColor GetResourceColor(string resourceName)
        {
            resourceName = resourceName.ToLowerInvariant();
            if (resourceName.Contains("shield") || resourceName.Contains("plasma"))
                return ConsoleColor.Cyan;
            if (resourceName.Contains("fuel"))
                return ConsoleColor.Red;
            if (resourceName.Contains("hull"))
                return ConsoleColor.DarkGreen;
            if (resourceName.Contains("weapon"))
                return ConsoleColor.Magenta;
            if (resourceName.Contains("engine"))
                return ConsoleColor.DarkCyan;
            if (resourceName.Contains("cargo"))
                return ConsoleColor.DarkYellow;
            return ConsoleColor.White;
        }
    }
}