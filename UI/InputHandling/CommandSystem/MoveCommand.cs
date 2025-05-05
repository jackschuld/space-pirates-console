using System;

namespace SpacePirates.Console.UI.InputHandling.CommandSystem
{
    public class MoveCommand : ICommand
    {
        public string Name => "move";
        public string ShortName => "m";
        public static string Description => "move x y or mxy (x: 1-75, y: a-z)";
        string ICommand.Description => Description;

        public void Execute(CommandContext context, string[] args)
        {
            if (args.Length != 2)
            {
                context.Result = Description;
                return;
            }

            // Parse x (1-75)
            if (!int.TryParse(args[0], out int x) || x < 1 || x > 75)
            {
                context.Result = "X must be a number between 1 and 75.";
                return;
            }

            // Parse y (a-z or A-Z)
            string yStr = args[1].ToLower();
            if (yStr.Length != 1 || yStr[0] < 'a' || yStr[0] > 'z')
            {
                context.Result = "Y must be a letter.";
                return;
            }
            int y = yStr[0] - 'a' + 1;

            context.Game.MoveShipTo(x, y);
            context.Result = $"Moved ship to ({x}, {yStr[0]})";
        }
    }
} 