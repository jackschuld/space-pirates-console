using SpacePirates.Console.Core.Models.State;
using SpacePirates.Console.Core.Interfaces;
using System;
using System.Threading;

namespace SpacePirates.Console.Game.Controllers
{
    public class ShieldController
    {
        private readonly GameState? _gameState;
        private readonly IRenderer _renderer;
        private DateTime _lastShieldChargeUpdate = DateTime.UtcNow;
        private double _shieldChargeProgress = 0;
        private readonly string _defaultHelpText = "Tab to toggle instructions | ESC to exit";

        public ShieldController(GameState? gameState, IRenderer renderer)
        {
            _gameState = gameState;
            _renderer = renderer;
        }

        public void HandleShieldKey()
        {
            if (_gameState?.PlayerShip?.Shield != null)
            {
                var shield = _gameState.PlayerShip.Shield;
                var ship = _gameState.PlayerShip;
                var api = AppDomain.CurrentDomain.GetData("ApiClient") as SpacePirates.Console.UI.Components.ApiClient;
                if (!shield.IsActive && !shield.Charging)
                {
                    shield.Charging = true;
                    shield.CurrentIntegrity = 0;
                    _renderer.SetHelpText("Shield charging...");
                    _renderer.EndFrame();
                    int maxCapacity = shield.CalculateMaxCapacity();
                    int chargeSteps = 20;
                    int msPerStep = 10000 / chargeSteps;
                    for (int i = 1; i <= chargeSteps; i++)
                    {
                        shield.CurrentIntegrity = (int)(maxCapacity * (i / (double)chargeSteps));
                        _renderer.SetHelpText($"Shield charging... {shield.CurrentIntegrity * 100 / maxCapacity}%");
                        _renderer.EndFrame();
                        Thread.Sleep(msPerStep);
                    }
                    shield.CurrentIntegrity = maxCapacity;
                    shield.IsActive = true;
                    shield.Charging = false;
                    _renderer.ShowMessage("Shield fully charged!", true);
                    _renderer.EndFrame();
                    Thread.Sleep(800);
                    _renderer.ShowMessage(_defaultHelpText);
                    // Persist shield state
                    if (api != null)
                    {
                        var dto = new {
                            Shield = new {
                                CurrentLevel = shield.CurrentLevel,
                                CurrentIntegrity = shield.CurrentIntegrity,
                                IsActive = shield.IsActive
                            }
                        };
                        _ = api.UpdateShipStateAsync(ship.Id, dto);
                    }
                }
                else if (shield.IsActive && !shield.Charging)
                {
                    shield.IsActive = false;
                    shield.CurrentIntegrity = 0;
                    _renderer.ShowMessage("Shield deactivated!", true);
                    _renderer.EndFrame();
                    Thread.Sleep(800);
                    _renderer.ShowMessage(_defaultHelpText);
                    // Persist shield state
                    if (api != null)
                    {
                        var dto = new {
                            Shield = new {
                                CurrentLevel = shield.CurrentLevel,
                                CurrentIntegrity = shield.CurrentIntegrity,
                                IsActive = shield.IsActive
                            }
                        };
                        _ = api.UpdateShipStateAsync(ship.Id, dto);
                    }
                }
            }
        }

        public void UpdateShieldCharging()
        {
            var ship = _gameState?.PlayerShip;
            if (ship?.Shield != null && ship.Shield.Charging)
            {
                var now = DateTime.UtcNow;
                double seconds = (now - _lastShieldChargeUpdate).TotalSeconds;
                _lastShieldChargeUpdate = now;
                int maxCapacity = ship.Shield.CalculateMaxCapacity();
                double percentPerSecond = 100.0 / 15.0;
                _shieldChargeProgress += percentPerSecond * seconds;
                int newPercent = (int)_shieldChargeProgress;
                newPercent = Math.Clamp(newPercent, 0, 100);
                ship.Shield.CurrentIntegrity = (int)(maxCapacity * (newPercent / 100.0));
                if (newPercent >= 100)
                {
                    ship.Shield.CurrentIntegrity = maxCapacity;
                    ship.Shield.IsActive = true;
                    ship.Shield.Charging = false;
                    _renderer.ShowMessage("Shield fully charged!", true);
                    _renderer.EndFrame();
                    Thread.Sleep(800);
                    _renderer.ShowMessage(_defaultHelpText);
                }
            }
        }
    }
} 