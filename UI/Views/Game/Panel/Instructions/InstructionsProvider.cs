using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.Game.Engine;
using SpacePirates.Console.UI.Views;
using System.Linq;
using SpacePirates.Console.UI.Renderers.PanelRenderer;
using SpacePirates.Console.UI.Views.Game.Panel.Instructions;

namespace SpacePirates.Console.UI.Views
{
    public class InstructionsProvider
    {
        public static (string[] Commands, (string Key, string Description)[] QuickKeys) GetInstructions(GameEngine.ControlState? controlState)
        {
            // Try to get instructions from the current map view if it implements IHasInstructions
            IHasInstructions? hasInstructions = null;
            if (SpacePirates.Console.UI.ConsoleRenderer.ConsoleRenderer.CurrentMapView is IHasInstructions mapViewInstructions)
                hasInstructions = mapViewInstructions;

            switch (controlState)
            {
                case GameEngine.ControlState.GalaxyMap:
                case GameEngine.ControlState.SolarSystemMap:
                    if (hasInstructions != null)
                    {
                        var commands = hasInstructions.Instructions;
                        var quickKeys = hasInstructions.QuickKeys.Concat(InstructionsHelper.GetDefaultQuickKeys()).ToArray();
                        return (commands, quickKeys);
                    }
                    else
                    {
                        return (InstructionsData.Commands, InstructionsData.QuickKeys);
                    }
                case GameEngine.ControlState.StartMenu:
                    return (new string[] { }, new[] { ("h/j/k/l", "Move selection") });
                default:
                    return (InstructionsData.Commands, InstructionsData.QuickKeys);
            }
        }
    }
}