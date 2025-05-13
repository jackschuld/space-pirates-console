using SpacePirates.Console.UI.Views;
using SpacePirates.API.Models;
using SpacePirates.Console.Core.Models.Movement;
using System;

namespace SpacePirates.Console.UI.Controls
{
    public class GalaxyControls : GameControls
    {
        public override async void HandleInput(ConsoleKeyInfo key, BaseView view)
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
                    var trailProp = engine.GetType().GetProperty("CurrentShipTrail");
                    var renderer = AppDomain.CurrentDomain.GetData("ConsoleRenderer");
                    var setHelpText = renderer?.GetType().GetMethod("SetHelpText");
                    var endFrame = renderer?.GetType().GetMethod("EndFrame");
                    var system = FindSolarSystem(input);
                    var trail = trailProp?.GetValue(engine);
                    if (system != null && view is SpacePirates.Console.UI.Views.GameView gameView)
                    {
                        if (((SpacePirates.API.Models.SolarSystem)system).Star == null)
                            System.Console.WriteLine("[DEBUG] Star property is null!");
                        gameView.SwitchToSolarSystem((SpacePirates.API.Models.SolarSystem)system, (SpacePirates.Console.Core.Models.Movement.ShipTrail?)trail);
                        var api = AppDomain.CurrentDomain.GetData("ApiClient") as SpacePirates.Console.UI.Components.ApiClient;
                        var starId = ((SpacePirates.API.Models.SolarSystem)system).Star?.Id;
                        if (api != null && starId != null)
                        {
                            var result = api.DiscoverStarAsync(starId.Value).GetAwaiter().GetResult();
                        }
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

        
        public SolarSystem? FindSolarSystem(string input)
        {
            var engine = AppDomain.CurrentDomain.GetData("GameEngine");
            if (engine == null) return null;
            var gameStateProp = engine.GetType().GetField("_gameState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gameState = gameStateProp?.GetValue(engine) as SpacePirates.Console.Core.Models.State.GameState;
            if (gameState == null || gameState.Galaxy == null) return null;

            if (int.TryParse(input, out int id))
                return gameState.Galaxy.SolarSystems.Find(s => s.Id == id);
            var exact = gameState.Galaxy.SolarSystems.Find(s => s.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;
            var endsWith = gameState.Galaxy.SolarSystems.Find(s =>
                s.Name.Contains("-") &&
                s.Name.Substring(s.Name.LastIndexOf('-') + 1).Equals(input, StringComparison.OrdinalIgnoreCase));
            if (endsWith != null) return endsWith;
            return gameState.Galaxy.SolarSystems.Find(s => s.Name.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
} 