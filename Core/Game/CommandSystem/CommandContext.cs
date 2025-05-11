using SpacePirates.Console.Game.Engine;
using SpacePirates.Console.Core.Interfaces;

namespace SpacePirates.Console.Game.CommandSystem
{
    public class CommandContext
    {
        public GameEngine Game { get; }
        public IRenderer Renderer { get; }
        public string? Result { get; set; }

        public CommandContext(GameEngine game, IRenderer renderer)
        {
            Game = game;
            Renderer = renderer;
            Result = null;
        }
    }
} 