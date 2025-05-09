using SpacePirates.Console.UI.Views;
using System;

namespace SpacePirates.Console.UI.Controls
{
    public class GalaxyControls : GameControls
    {
        public override void HandleInput(ConsoleKeyInfo key, BaseView view)
        {
            if (char.ToLower(key.KeyChar) == 'w')
            {
                var engine = AppDomain.CurrentDomain.GetData("GameEngine");
                if (engine == null) return;
                var method = engine.GetType().GetMethod("CommandInputLoop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method == null) return;
                string? input = method.Invoke(engine, new object[] { "Warp to System ID or Name: " }) as string;
                if (!string.IsNullOrWhiteSpace(input))
                {
                    var findMethod = engine.GetType().GetMethod("FindSolarSystem");
                    var trailProp = engine.GetType().GetProperty("CurrentShipTrail");
                    var renderer = AppDomain.CurrentDomain.GetData("ConsoleRenderer");
                    var setHelpText = renderer?.GetType().GetMethod("SetHelpText");
                    var endFrame = renderer?.GetType().GetMethod("EndFrame");
                    var system = findMethod?.Invoke(engine, new object[] { input });
                    var trail = trailProp?.GetValue(engine);
                    if (system != null && view is SpacePirates.Console.UI.Views.GameView gameView)
                    {
                        gameView.SwitchToSolarSystem((SpacePirates.API.Models.SolarSystem)system, (SpacePirates.Console.Core.Models.Movement.ShipTrail?)trail);
                        var setTempNotif = renderer?.GetType().GetMethod("SetTemporaryNotification");
                        setTempNotif?.Invoke(renderer, new object[] { $"Warped to {((SpacePirates.API.Models.SolarSystem)system).Name}!" });
                        endFrame?.Invoke(renderer, null);
                    }
                    else
                    {
                        var setTempNotif = renderer?.GetType().GetMethod("SetTemporaryNotification");
                        setTempNotif?.Invoke(renderer, new object[] { $"Solar system not found: {input}" });
                        endFrame?.Invoke(renderer, null);
                    }
                }
                return;
            }
            base.HandleInput(key, view);
        }
    }
} 