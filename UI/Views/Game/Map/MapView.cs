using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.UI.Components;
using System;
using SpacePirates.Console.UI.Views;

namespace SpacePirates.Console.UI.Views.Map
{
    public abstract class MapView : BaseView, IGameComponent, IHasInstructions
    {
        protected readonly (int X, int Y, int Width, int Height) _bounds;
        protected int _cursorX;
        protected int _cursorY;
        protected bool _showDetails = false;

        public MapView((int X, int Y, int Width, int Height) bounds)
        {
            _bounds = bounds;
            _cursorX = _bounds.X + _bounds.Width / 2;
            _cursorY = _bounds.Y + _bounds.Height / 2;
        }

        public (int X, int Y, int Width, int Height) Bounds => _bounds;
        public (int X, int Y) CursorPosition => (_cursorX, _cursorY);

        protected bool IsUnderCursor(int x, int y) => x == _cursorX && y == _cursorY;

        protected void MoveCursor(int dx, int dy)
        {
            _cursorX = Math.Max(_bounds.X + 1, Math.Min(_bounds.X + _bounds.Width - 2, _cursorX + dx));
            _cursorY = Math.Max(_bounds.Y + 1, Math.Min(_bounds.Y + _bounds.Height - 2, _cursorY + dy));
        }

        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            switch (char.ToLower(keyInfo.KeyChar))
            {
                case 'h':
                    MoveCursor(-1, 0);
                    break;
                case 'l':
                    MoveCursor(1, 0);
                    break;
                case 'k':
                    MoveCursor(0, -1);
                    break;
                case 'j':
                    MoveCursor(0, 1);
                    break;
            }
        }

        public override void Render()
        {
            // This method is required by BaseView, but actual rendering uses Render(IBufferWriter buffer)
            // You may want to throw NotImplementedException or leave empty if not used directly
        }

        public virtual void Render(IBufferWriter buffer)
        {
            // Draw border in dark yellow for consistency
            if (buffer is SpacePirates.Console.UI.ConsoleRenderer.ConsoleBufferWriter cbw)
                cbw.DrawBox(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, BoxStyle.Double, PanelStyles.BorderColor);
            else
                buffer.DrawBox(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, BoxStyle.Double);
            RenderMapObjects(buffer);
            if (_showDetails)
                RenderDetailsPanel(buffer);

            // Draw X axis numbers (1-75) one row below the border
            int numbersY = _bounds.Y + _bounds.Height; // outside the border
            int xStart = _bounds.X + 1;
            int maxX = _bounds.Width - 2;
            int[] xLabels = { 1, 12, 23, 34, 45, 56, 67, 75 };
            foreach (int x in xLabels)
            {
                int drawX = xStart + x - 1;
                string label = x.ToString();
                for (int j = 0; j < label.Length; j++)
                {
                    buffer.DrawString(drawX + j, numbersY, label[j].ToString(), ConsoleColor.White);
                }
            }

            // Draw Y axis letters (A-Z) one column to the right of the border
            int lettersX = _bounds.X + _bounds.Width; // outside the border
            int lettersStartY = _bounds.Y + 1;
            int letterAreaHeight = _bounds.Height - 2;
            int[] letterIndices = { 0, 5, 10, 15, 20, 25 }; // A, F, K, P, U, Z
            for (int i = 0; i < letterIndices.Length; i++)
            {
                int letterIndex = letterIndices[i];
                char letter = (char)('A' + letterIndex);
                int y = lettersStartY + (int)Math.Round(i * (letterAreaHeight - 1) / (double)(letterIndices.Length - 1));
                buffer.DrawString(lettersX, y, letter.ToString(), ConsoleColor.White);
            }
        }

        protected abstract void RenderMapObjects(IBufferWriter buffer);
        protected abstract void RenderDetailsPanel(IBufferWriter buffer);

        public virtual void Update(IGameState gameState) { }

        public void HideDetails() => _showDetails = false;

        public virtual string[] Instructions => new string[0];
        public virtual (string Key, string Description)[] QuickKeys => new (string, string)[0];
    }
} 