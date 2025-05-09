using SpacePirates.Console.UI.Views;
using System;

namespace SpacePirates.Console.UI.Controls
{
    public class SolarSystemControls : GameControls
    {
        public override void HandleInput(ConsoleKeyInfo key, BaseView view)
        {
            switch (char.ToLower(key.KeyChar))
            {
                case 'g':
                    // Galaxy switch logic
                    var engine = AppDomain.CurrentDomain.GetData("GameEngine");
                    if (engine == null) return;
                    var galaxyProp = engine.GetType().GetProperty("CurrentGalaxy");
                    var trailProp = engine.GetType().GetProperty("CurrentShipTrail");
                    var renderer = AppDomain.CurrentDomain.GetData("ConsoleRenderer");
                    var setHelpText = renderer?.GetType().GetMethod("SetHelpText");
                    var endFrame = renderer?.GetType().GetMethod("EndFrame");
                    var galaxy = galaxyProp?.GetValue(engine) as SpacePirates.API.Models.Galaxy;
                    var trail = trailProp?.GetValue(engine) as SpacePirates.Console.Core.Models.Movement.ShipTrail;
                    if (galaxy != null && view is SpacePirates.Console.UI.Views.GameView gameView)
                    {
                        gameView.SwitchToGalaxy(galaxy, trail);
                        setHelpText?.Invoke(renderer, new object[] { $"Returned to galaxy view." });
                        endFrame?.Invoke(renderer, null);
                    }
                    else
                    {
                        setHelpText?.Invoke(renderer, new object[] { $"Galaxy not found." });
                        endFrame?.Invoke(renderer, null);
                    }
                    break;
                case 'm':
                    // Move command logic
                    var engineM = AppDomain.CurrentDomain.GetData("GameEngine");
                    if (engineM == null) return;
                    var method = engineM.GetType().GetMethod("CommandInputLoop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var moveMethod = engineM.GetType().GetMethod("MoveShipTo");
                    if (method == null || moveMethod == null) return;
                    string? input = method.Invoke(engineM, new object[] { "Move: " }) as string;
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
                            moveMethod.Invoke(engineM, new object[] { x, y });
                        }
                        else
                        {
                            var rendererM = AppDomain.CurrentDomain.GetData("ConsoleRenderer");
                            var setHelpTextM = rendererM?.GetType().GetMethod("SetHelpText");
                            setHelpTextM?.Invoke(rendererM, new object[] { "Invalid coordinates. Use: x y (e.g. 12 A or 12A)" });
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
                            setHelpTextS?.Invoke(rendererS, new object[] { "Shield charging..." });
                            endFrameS?.Invoke(rendererS, null);
                            int maxCapacity = (int)calcMaxCapMethod.Invoke(shield, null)!;
                            int chargeSteps = 20;
                            int msPerStep = 10000 / chargeSteps;
                            for (int i = 1; i <= chargeSteps; i++)
                            {
                                currentIntegrityProp.SetValue(shield, (int)(maxCapacity * (i / (double)chargeSteps)));
                                setHelpTextS?.Invoke(rendererS, new object[] { $"Shield charging... {((int)currentIntegrityProp.GetValue(shield)! * 100 / maxCapacity)}%" });
                                endFrameS?.Invoke(rendererS, null);
                                System.Threading.Thread.Sleep(msPerStep);
                            }
                            currentIntegrityProp.SetValue(shield, maxCapacity);
                            isActiveProp.SetValue(shield, true);
                            chargingProp.SetValue(shield, false);
                            setHelpTextS?.Invoke(rendererS, new object[] { "Shield fully charged!" });
                            endFrameS?.Invoke(rendererS, null);
                            System.Threading.Thread.Sleep(800);
                            setHelpTextS?.Invoke(rendererS, new object[] { "Tab to toggle instructions | ESC to exit" });
                            endFrameS?.Invoke(rendererS, null);
                        }
                        else if (isActive && !charging)
                        {
                            isActiveProp.SetValue(shield, false);
                            currentIntegrityProp.SetValue(shield, 0);
                            setHelpTextS?.Invoke(rendererS, new object[] { "Shield deactivated!" });
                            endFrameS?.Invoke(rendererS, null);
                            System.Threading.Thread.Sleep(800);
                            setHelpTextS?.Invoke(rendererS, new object[] { "Tab to toggle instructions | ESC to exit" });
                            endFrameS?.Invoke(rendererS, null);
                        }
                    }
                    break;
                default:
                    base.HandleInput(key, view);
                    break;
            }
        }
    }
} 