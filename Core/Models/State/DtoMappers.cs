using SpacePirates.API.Models;
using SpacePirates.API.Models.DTOs;
using SpacePirates.API.Models.ShipComponents;

namespace SpacePirates.Console.Core.Models.State
{
    public static class DtoMappers
    {
        public static Ship MapShipDtoToShip(ShipDto dto)
        {
            return new Ship
            {
                Id = dto.Id,
                Name = dto.Name,
                CaptainName = dto.CaptainName,
                Credits = dto.Credits,
                Position = dto.Position == null ? new Position() : new Position { X = dto.Position.X, Y = dto.Position.Y },
                FuelSystem = dto.FuelSystem == null ? new FuelSystem() : new FuelSystem
                {
                    CurrentLevel = dto.FuelSystem.CurrentLevel,
                    CurrentFuel = dto.FuelSystem.CurrentFuel
                },
                Shield = dto.Shield == null ? new Shield() : new Shield
                {
                    CurrentLevel = dto.Shield.CurrentLevel,
                    CurrentIntegrity = dto.Shield.CurrentIntegrity,
                    IsActive = dto.Shield.IsActive
                },
                Hull = dto.Hull == null ? new Hull() : new Hull
                {
                    CurrentLevel = dto.Hull.CurrentLevel,
                    CurrentIntegrity = dto.Hull.CurrentIntegrity
                },
                Engine = dto.Engine == null ? new Engine() : new Engine
                {
                    CurrentLevel = dto.Engine.CurrentLevel
                },
                CargoSystem = dto.CargoSystem == null ? new CargoSystem() : new CargoSystem
                {
                    CurrentLevel = dto.CargoSystem.CurrentLevel,
                    CurrentLoad = dto.CargoSystem.CurrentLoad
                },
                WeaponSystem = dto.WeaponSystem == null ? new WeaponSystem() : new WeaponSystem
                {
                    CurrentLevel = dto.WeaponSystem.CurrentLevel
                }
            };
        }
    }
} 