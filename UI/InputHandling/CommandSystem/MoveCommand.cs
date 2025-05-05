using System;

namespace SpacePirates.Console.UI.InputHandling.CommandSystem
{
    public class MoveCommand : ICommand
    {
        public string Name => "move";
        public string Description => "Move the ship to a specific location. Usage: :move x y";

        public void Execute(CommandContext context, string[] args)
        {
            if (args.Length != 2 || !int.TryParse(args[0], out int x) || !int.TryParse(args[1], out int y))
            {
                context.Result = "Usage: :move x y";
                return;
            }
            context.Game.MoveShipTo(x, y);
            context.Result = $"Moved ship to ({x}, {y})";
        }
    }
} 