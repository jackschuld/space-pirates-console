using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using SpacePirates.API.Models;

namespace SpacePirates.Console.UI.Components
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public ApiClient(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _http = new HttpClient();
        }

        public async Task<List<GameSummary>> ListGamesAsync()
        {
            var resp = await _http.GetAsync($"{_baseUrl}/api/game/list");
            if (!resp.IsSuccessStatusCode) return new List<GameSummary>();
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<GameSummary>>(json) ?? new List<GameSummary>();
        }

        public async Task<GameStateDto?> StartNewGameAsync(string captainName, string shipName)
        {
            var req = new { CaptainName = captainName, ShipName = shipName };
            var resp = await _http.PostAsJsonAsync($"{_baseUrl}/api/game/start", req);
            if (!resp.IsSuccessStatusCode)
            {
                System.Console.WriteLine($"API returned error: {resp.StatusCode}");
                return null;
            }
            var json = await resp.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            try
            {
                return JsonSerializer.Deserialize<GameStateDto>(json, options);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[ERROR] Failed to deserialize GameStartResponse: {ex.Message}");
                return null;
            }
        }

        public async Task<GameStateDto?> LoadGameAsync(int gameId)
        {
            var resp = await _http.GetAsync($"{_baseUrl}/api/game/{gameId}");
            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<GameStateDto>(json, options);
        }

        public async Task<bool> DeleteGameAsync(int gameId)
        {
            var resp = await _http.DeleteAsync($"{_baseUrl}/api/game/{gameId}");
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DiscoverStarAsync(int starId)
        {
            try
            {
                var resp = await _http.PostAsync($"{_baseUrl}/api/game/discover-star/{starId}", null);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[ERROR] DiscoverStarAsync exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> InspectPlanetAsync(int planetId)
        {
            try
            {
                var resp = await _http.PostAsync($"{_baseUrl}/api/game/inspect-planet/{planetId}", null);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<SolarSystem?> GetSolarSystemAsync(int systemId)
        {
            try
            {
                var resp = await _http.GetAsync($"{_baseUrl}/api/game/solar-system/{systemId}");
                if (!resp.IsSuccessStatusCode) return null;
                var json = await resp.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<SolarSystem>(json, options);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[ERROR] GetSolarSystemAsync exception: {ex.Message}");
                return null;
            }
        }

        public async Task PostAsync(string url, object data)
        {
            using var client = new HttpClient();
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> UpdateShipFuelAsync(int shipId, double percentFuel)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync($"{_baseUrl}/api/game/update-ship-fuel/{shipId}", percentFuel);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[ERROR] UpdateShipFuelAsync exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateShipStateAsync(int shipId, object shipUpdateDto)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync($"{_baseUrl}/api/game/update-ship-state/{shipId}", shipUpdateDto);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[ERROR] UpdateShipStateAsync exception: {ex.Message}");
                return false;
            }
        }

        public async Task<dynamic?> MinePlanetResourceAsync(int planetId, int resourceId, int amount, int shipId)
        {
            var req = new {
                PlanetId = planetId,
                ResourceId = resourceId,
                Amount = amount,
                ShipId = shipId
            };
            var resp = await _http.PostAsJsonAsync($"{_baseUrl}/api/game/mine-planet-resource", req);
            if (!resp.IsSuccessStatusCode)
            {
                System.Console.WriteLine($"API returned error: {resp.StatusCode}");
                return null;
            }
            var json = await resp.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<dynamic>(json, options);
        }
    }

    public class GameSummary
    {
        public int gameId { get; set; }
        public string shipName { get; set; } = string.Empty;
        public string captainName { get; set; } = string.Empty;
        public string galaxyName { get; set; } = string.Empty;
        public DateTime lastPlayed { get; set; }
    }

    public class GameStartResponse
    {
        public int gameId { get; set; }
        public int shipId { get; set; }
        public int galaxyId { get; set; }
        public string shipName { get; set; } = string.Empty;
        public string captainName { get; set; } = string.Empty;
        public string galaxyName { get; set; } = string.Empty;
    }

    public class GameStateDto
    {
        public int gameId { get; set; }
        public Ship? ship { get; set; }
        public Galaxy? galaxy { get; set; }
        // Add more fields as needed for your game state
        public Ship? GetShip() => ship;
        public Galaxy? GetGalaxy() => galaxy;
    }
} 