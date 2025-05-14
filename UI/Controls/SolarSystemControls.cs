using SpacePirates.Console.UI.Views;
using System;
using System.Linq;
using SpacePirates.Console.UI.Helpers;
namespace SpacePirates.Console.UI.Controls
{
    public class SolarSystemControls : GameControls
    {
        public override void HandleInput(ConsoleKeyInfo key, BaseView view)
        {
            switch (char.ToLower(key.KeyChar))
            {
                case 'r':
                    // Return to galaxy view switch logic
                    var engine = AppDomain.CurrentDomain.GetData("GameEngine");
                    if (engine == null) return;
                    var galaxyProp = engine.GetType().GetProperty("CurrentGalaxy");
                    var trailProp = engine.GetType().GetProperty("CurrentShipTrail");
                    var renderer = AppDomain.CurrentDomain.GetData("ConsoleRenderer");
                    var setHelpText = renderer?.GetType().GetMethod("SetHelpText");
                    var showMessage = renderer?.GetType().GetMethod("ShowMessage");
                    var endFrame = renderer?.GetType().GetMethod("EndFrame");
                    var galaxy = galaxyProp?.GetValue(engine) as SpacePirates.API.Models.Galaxy;
                    var trail = trailProp?.GetValue(engine) as SpacePirates.Console.Core.Models.Movement.ShipTrail;
                    if (galaxy != null && view is SpacePirates.Console.UI.Views.GameView gameView)
                    {
                        gameView.SwitchToGalaxy(galaxy, trail);
                        showMessage?.Invoke(renderer, new object[] { $"Returned to galaxy view.", true });
                        endFrame?.Invoke(renderer, null);
                    }
                    else
                    {
                        showMessage?.Invoke(renderer, new object[] { $"Galaxy not found.", true });
                        endFrame?.Invoke(renderer, null);
                    }
                    break;
                case 'f':
                    // Fly command logic
                    var engineM = AppDomain.CurrentDomain.GetData("GameEngine");
                    if (engineM == null) return;
                    var method = engineM.GetType().GetMethod("CommandInputLoop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var flyMethod = engineM.GetType().GetMethod("FlyShipTo");
                    if (method == null || flyMethod == null) return;
                    string? input = method.Invoke(engineM, new object[] { "Fly: " }) as string;
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        input = input.Replace(",", " ").Replace("-", " ").Trim();
                        string xPart = string.Empty, yPart = string.Empty;
                        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            xPart = parts[0];
                            yPart = parts[1];
                        }
                        else if (parts.Length == 1)
                        {
                            var s = parts[0];
                            int i = 0;
                            while (i < s.Length && char.IsDigit(s[i])) i++;
                            if (i > 0 && i < s.Length)
                            {
                                xPart = s.Substring(0, i);
                                yPart = s.Substring(i);
                            }
                        }
                        if (int.TryParse(xPart, out int x) && yPart.Length == 1 && char.IsLetter(yPart[0]))
                        {
                            int y = char.ToUpper(yPart[0]) - 'A' + 1;
                            flyMethod.Invoke(engineM, new object[] { x, y });
                            // After flying, update ship fuel in backend
                            var gameStatePropFly = engineM.GetType().GetField("_gameState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            var gameStateFly = gameStatePropFly?.GetValue(engineM);
                            var playerShipPropFly = gameStateFly?.GetType().GetProperty("PlayerShip");
                            var playerShipFly = playerShipPropFly?.GetValue(gameStateFly);
                            var shipIdProp = playerShipFly?.GetType().GetProperty("Id");
                            var fuelSystemProp = playerShipFly?.GetType().GetProperty("FuelSystem");
                            var fuelSystem = fuelSystemProp?.GetValue(playerShipFly);
                            var currentFuelProp = fuelSystem?.GetType().GetProperty("CurrentFuel");
                            if (shipIdProp != null && currentFuelProp != null)
                            {
                                int shipId = (int)shipIdProp.GetValue(playerShipFly);
                                double currentFuel = Convert.ToDouble(currentFuelProp.GetValue(fuelSystem));
                                var api = AppDomain.CurrentDomain.GetData("ApiClient") as SpacePirates.Console.UI.Components.ApiClient;
                                if (api != null)
                                {
                                    _ = api.UpdateShipFuelAsync(shipId, currentFuel);
                                }
                            }
                        }
                        else
                        {
                            var rendererM = AppDomain.CurrentDomain.GetData("ConsoleRenderer");
                            var showMessageM = rendererM?.GetType().GetMethod("ShowMessage");
                            showMessageM?.Invoke(rendererM, new object[] { "Invalid coordinates. Use: x y (e.g. 12 A or 12A)", true });
                        }
                    }
                    break;
                case 's':
                    // Shield command logic
                    var engineS = AppDomain.CurrentDomain.GetData("GameEngine");
                    if (engineS == null) return;
                    var gameStateProp = engineS.GetType().GetField("_gameState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var rendererS = AppDomain.CurrentDomain.GetData("ConsoleRenderer");
                    var setHelpTextS = rendererS?.GetType().GetMethod("SetHelpText");
                    var showMessageS = rendererS?.GetType().GetMethod("ShowMessage");
                    var endFrameS = rendererS?.GetType().GetMethod("EndFrame");
                    var gameState = gameStateProp?.GetValue(engineS);
                    var playerShipProp = gameState?.GetType().GetProperty("PlayerShip");
                    var playerShip = playerShipProp?.GetValue(gameState);
                    var shieldProp = playerShip?.GetType().GetProperty("Shield");
                    var shield = shieldProp?.GetValue(playerShip);
                    var isActiveProp = shield?.GetType().GetProperty("IsActive");
                    var chargingProp = shield?.GetType().GetProperty("Charging");
                    var currentIntegrityProp = shield?.GetType().GetProperty("CurrentIntegrity");
                    var calcMaxCapMethod = shield?.GetType().GetMethod("CalculateMaxCapacity");
                    if (shield != null && isActiveProp != null && chargingProp != null && currentIntegrityProp != null && calcMaxCapMethod != null)
                    {
                        bool isActive = (bool)isActiveProp.GetValue(shield)!;
                        bool charging = (bool)chargingProp.GetValue(shield)!;
                        if (!isActive && !charging)
                        {
                            chargingProp.SetValue(shield, true);
                            currentIntegrityProp.SetValue(shield, 0);
                            showMessageS?.Invoke(rendererS, new object[] { "Shield charging...", false });
                            endFrameS?.Invoke(rendererS, null);
                            int maxCapacity = (int)calcMaxCapMethod.Invoke(shield, null)!;
                            int chargeSteps = 20;
                            int msPerStep = 10000 / chargeSteps;
                            for (int i = 1; i <= chargeSteps; i++)
                            {
                                currentIntegrityProp.SetValue(shield, (int)(maxCapacity * (i / (double)chargeSteps)));
                                showMessageS?.Invoke(rendererS, new object[] { $"Shield charging... {((int)currentIntegrityProp.GetValue(shield)! * 100 / maxCapacity)}%", false });
                                endFrameS?.Invoke(rendererS, null);
                                System.Threading.Thread.Sleep(msPerStep);
                            }
                            currentIntegrityProp.SetValue(shield, maxCapacity);
                            isActiveProp.SetValue(shield, true);
                            chargingProp.SetValue(shield, false);
                            showMessageS?.Invoke(rendererS, new object[] { "Shield fully charged!", true });
                            endFrameS?.Invoke(rendererS, null);
                            System.Threading.Thread.Sleep(800);
                            showMessageS?.Invoke(rendererS, new object[] { "Tab to toggle instructions | ESC to exit", false });
                            endFrameS?.Invoke(rendererS, null);
                        }
                        else if (isActive && !charging)
                        {
                            isActiveProp.SetValue(shield, false);
                            currentIntegrityProp.SetValue(shield, 0);
                            showMessageS?.Invoke(rendererS, new object[] { "Shield deactivated!", true });
                            endFrameS?.Invoke(rendererS, null);
                            System.Threading.Thread.Sleep(800);
                            showMessageS?.Invoke(rendererS, new object[] { "Tab to toggle instructions | ESC to exit", false });
                            endFrameS?.Invoke(rendererS, null);
                        }
                    }
                    break;
                case 'e':
                    // Support both direct SolarSystemMapView and GameView with Map as SolarSystemMapView
                    SpacePirates.Console.UI.Views.SolarSystemMapView? ssmvInspect = null;
                    if (view is SpacePirates.Console.UI.Views.SolarSystemMapView direct)
                        ssmvInspect = direct;
                    else if (view is SpacePirates.Console.UI.Views.GameView gv && gv.Map is SpacePirates.Console.UI.Views.SolarSystemMapView mapView)
                        ssmvInspect = mapView;
                    if (ssmvInspect == null) return;
                    var engineI = AppDomain.CurrentDomain.GetData("GameEngine");
                    if (engineI == null) return;
                    var methodI = engineI.GetType().GetMethod("CommandInputLoop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (methodI == null) return;
                    string? inputI = methodI.Invoke(engineI, new object[] { "Examine Planet (4-digit code): " }) as string;
                    if (!string.IsNullOrWhiteSpace(inputI) && inputI.Length == 4)
                    {
                        var planetInspect = ssmvInspect.System.Planets.FirstOrDefault(p => p.Name.Length > 5 && p.Name[^4..].Equals(inputI, StringComparison.OrdinalIgnoreCase));
                        if (planetInspect != null)
                        {
                            var api = AppDomain.CurrentDomain.GetData("ApiClient") as SpacePirates.Console.UI.Components.ApiClient;
                            if (api != null)
                            {
                                _ = InspectPlanetAsync(api, planetInspect, view);
                            }
                            else
                            {
                                System.Console.WriteLine("[ERROR] ApiClient not found in AppDomain.");
                            }
                        }
                        else
                        {
                            System.Console.WriteLine($"No planet found with code '{inputI}' in this system.");
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("Invalid planet code. Enter the 4-digit code after 'Planet-'.");
                    }
                    break;
                case 'd':
                    DrillPlanetAsync(view);
                    break;
                default:
                    base.HandleInput(key, view);
                    break;
            }
        }

        private async System.Threading.Tasks.Task InspectPlanetAsync(SpacePirates.Console.UI.Components.ApiClient api, SpacePirates.API.Models.Planet planet, BaseView? view = null)
        {
            bool success = await api.InspectPlanetAsync(planet.Id);
            if (success)
            {
                if (view is SpacePirates.Console.UI.Views.GameView gv && gv.Map is SpacePirates.Console.UI.Views.SolarSystemMapView ssmv)
                {
                    var updatedSystem = await api.GetSolarSystemAsync(ssmv.System.Id);
                    if (updatedSystem != null)
                    {
                        ssmv.System.UpdateFrom(updatedSystem);
                        if (gv.Panel is SpacePirates.Console.UI.Views.SolarSystemStatusView sysPanel)
                        {
                            sysPanel.SetSystem(updatedSystem);
                        }
                    }
                    var renderer = AppDomain.CurrentDomain.GetData("ConsoleRenderer");
                    var setTempNotif = renderer?.GetType().GetMethod("SetTemporaryNotification");
                    setTempNotif?.Invoke(renderer, new object[] { $"Planet '{planet.Name}' examined and marked as discovered." });
                    gv.Panel?.Render();
                }
            }
            else
            {
                var renderer = AppDomain.CurrentDomain.GetData("ConsoleRenderer");
                var setTempNotif = renderer?.GetType().GetMethod("SetTemporaryNotification");
                setTempNotif?.Invoke(renderer, new object[] { $"Failed to examine planet '{planet.Name}'." });
            }
        }

        private async void DrillPlanetAsync(BaseView view)
        {
            try
            {
                var engineD = AppDomain.CurrentDomain.GetData("GameEngine");
                if (engineD == null) return;
                var gameStatePropD = engineD.GetType().GetField("_gameState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var rendererD = AppDomain.CurrentDomain.GetData("ConsoleRenderer");
                var showMessageD = rendererD?.GetType().GetMethod("ShowMessage");
                var endFrameD = rendererD?.GetType().GetMethod("EndFrame");
                var gameStateD = gameStatePropD?.GetValue(engineD);
                var playerShipPropD = gameStateD?.GetType().GetProperty("PlayerShip");
                var playerShipD = playerShipPropD?.GetValue(gameStateD);
                var shipPosProp = playerShipD?.GetType().GetProperty("Position");
                var shipPos = shipPosProp?.GetValue(playerShipD);
                var shipXProp = shipPos?.GetType().GetProperty("X");
                var shipYProp = shipPos?.GetType().GetProperty("Y");
                int shipX = shipXProp != null ? (int)Math.Round(Convert.ToDouble(shipXProp.GetValue(shipPos))) : -1;
                int shipY = shipYProp != null ? (int)Math.Round(Convert.ToDouble(shipYProp.GetValue(shipPos))) : -1;

                // Get the current SolarSystem and its planets
                SpacePirates.API.Models.SolarSystem? system = null;
                (int X, int Y, int Width, int Height) bounds = (0, 0, 0, 0);
                if (view is SpacePirates.Console.UI.Views.SolarSystemMapView ssmvDrill)
                {
                    system = ssmvDrill.System;
                    bounds = ssmvDrill.Bounds;
                }
                else if (view is SpacePirates.Console.UI.Views.GameView gv && gv.Map is SpacePirates.Console.UI.Views.SolarSystemMapView mapViewD)
                {
                    system = mapViewD.System;
                    bounds = mapViewD.Bounds;
                }
                if (system == null)
                {
                    showMessageD?.Invoke(rendererD, new object[] { "No solar system context found.", true });
                    endFrameD?.Invoke(rendererD, null);
                    return;
                }

                // Calculate map coordinates for the ship (as rendered)
                int mapShipX = bounds.X + shipX;
                int mapShipY = bounds.Y + shipY;

                // Find planet at ship's position (within atmosphere)
                const int DRILL_RADIUS = 5;
                var planetToDrill = system.Planets
                    .FirstOrDefault(p => Math.Sqrt(Math.Pow((int)Math.Round(p.X) - mapShipX, 2) + Math.Pow((int)Math.Round(p.Y) - mapShipY, 2)) <= DRILL_RADIUS);
                if (planetToDrill == null)
                {
                    showMessageD?.Invoke(rendererD, new object[] { "You must within the atmosphere of a planet to drill. Type 'f' followed by the coordinates to fly to the planet.", true });
                    endFrameD?.Invoke(rendererD, null);
                    return;
                } else if (!planetToDrill.IsDiscovered) {
                    showMessageD?.Invoke(rendererD, new object[] { "You must discover the planet before you can drill it. Type 'e' followed by the 4-digit id to examine the planet.", true });
                    endFrameD?.Invoke(rendererD, null);
                    return;
                }

                // Simulate mining each resource
                var apiClient = AppDomain.CurrentDomain.GetData("ApiClient") as SpacePirates.Console.UI.Components.ApiClient;
                var cargoSystem = ((dynamic)playerShipD).CargoSystem;
                foreach (var res in planetToDrill.Resources)
                {
                    int originalAmount = res.AmountAvailable;
                    if (originalAmount <= 0) continue;
                    // Cargo space check
                    if (cargoSystem != null)
                    {
                        int maxCapacity = cargoSystem.CalculateMaxCapacity();

                        int currentLoad = cargoSystem.CurrentLoad;
                        int remainingSpace = maxCapacity - currentLoad;
                        if (originalAmount > remainingSpace)
                        {
                            showMessageD?.Invoke(rendererD, new object[] { $"Not enough cargo space to mine {ResourceHelper.GetResourceName(res.Resource.Name)}. {remainingSpace} units free.", true });
                            endFrameD?.Invoke(rendererD, null);
                            System.Threading.Thread.Sleep(1500);
                            break; // Stop drilling further resources
                        }
                    }
                    int steps = 10;
                    int msPerStep = 750; // .75 seconds per 10%
                    for (int i = 1; i <= steps; i++)
                    {
                        int percent = i * 10;
                        showMessageD?.Invoke(rendererD, new object[] { $"Drilling {ResourceHelper.GetResourceName(res.Resource.Name)}... {percent}%", false });
                        endFrameD?.Invoke(rendererD, null);
                        System.Threading.Thread.Sleep(msPerStep);
                    }
                    // Call backend to update planet and ship cargo
                    if (apiClient != null)
                    {
                        bool success = await apiClient.MinePlanetResourceAsync(planetToDrill.Id, res.Resource.Id, originalAmount, ((dynamic)playerShipD).Id);
                        if (!success)
                        {
                            showMessageD?.Invoke(rendererD, new object[] { $"[ERROR] Failed to update backend for mining {ResourceHelper.GetResourceName(res.Resource.Name)}.", true });
                            endFrameD?.Invoke(rendererD, null);
                        }
                        else
                        {
                            showMessageD?.Invoke(rendererD, new object[] { $"Mined {originalAmount} units of {ResourceHelper.GetResourceName(res.Resource.Name)}! (Synced)", true });
                            endFrameD?.Invoke(rendererD, null);
                            // Update local resource value
                            res.AmountAvailable -= originalAmount;
                            // Update local ship cargo (if possible)
                            if (cargoSystem != null)
                            {
                                // Find cargo item for this resource without using a lambda on dynamic
                                object cargoItem = null;
                                foreach (var ci in cargoSystem.CargoItems)
                                {
                                    if (ci.ResourceId == res.Resource.Id)
                                    {
                                        cargoItem = ci;
                                        break;
                                    }
                                }
                                if (cargoItem != null)
                                    ((dynamic)cargoItem).Amount += originalAmount;
                                else
                                    cargoSystem.CargoItems.Add(new SpacePirates.API.Models.ShipComponents.CargoItem { ResourceId = res.Resource.Id, Amount = originalAmount, Resource = res.Resource });
                                // Always recalculate CurrentLoad as the sum of all cargo item amounts (no lambda on dynamic)
                                int total = 0;
                                foreach (var ci in cargoSystem.CargoItems)
                                    total += ci.Amount;
                                cargoSystem.CurrentLoad = total;
                            }
                            // Re-render the ship status panel if present
                            if (view is SpacePirates.Console.UI.Views.GameView gameView &&
                                gameView.Panel is SpacePirates.Console.UI.Views.ShipStatusView shipStatus)
                            {
                                var localGameState = gameStatePropD?.GetValue(engineD) as SpacePirates.Console.Core.Interfaces.IGameState;
                                if (localGameState != null)
                                    shipStatus.UpdateStatus(localGameState);
                                shipStatus.Render();
                            }
                        }
                    }
                    System.Threading.Thread.Sleep(800);
                }
                showMessageD?.Invoke(rendererD, new object[] { "Drilling complete! Tab to toggle instructions | ESC to exit", false });
                endFrameD?.Invoke(rendererD, null);
            }
            catch (Exception ex)
            {
                var rendererD = AppDomain.CurrentDomain.GetData("ConsoleRenderer");
                var showMessageD = rendererD?.GetType().GetMethod("ShowMessage");
                var endFrameD = rendererD?.GetType().GetMethod("EndFrame");
                showMessageD?.Invoke(rendererD, new object[] { $"[ERROR] {ex.Message}", true });
                endFrameD?.Invoke(rendererD, null);
                System.Console.WriteLine(ex);
            }
        }
    }
} 