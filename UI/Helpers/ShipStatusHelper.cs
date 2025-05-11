namespace SpacePirates.Console.UI.Views
{
    public static class ShipStatusHelper
    {
        public static string RenderBar(double percent, int width, char fillChar, char emptyChar)
        {
            int fill = (int)(percent / 100.0 * width);
            if (fill < 0) fill = 0;
            if (fill > width) fill = width;
            return new string(fillChar, fill) + new string(emptyChar, width - fill);
        }
    }
} 