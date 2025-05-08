namespace SpacePirates.Console.UI.Utils
{
    public static class StatusComponentHelpers
    {
        public static string RenderBar(double percent, int width, char fillChar, char emptyChar)
        {
            int filled = (int)(width * (percent / 100.0));
            filled = Math.Clamp(filled, 0, width);
            return "[" + new string(fillChar, filled) + new string(emptyChar, width - filled) + "]";
        }
    }
} 