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
            System.Console.WriteLine($"[DEBUG] ApiClient baseUrl: {_baseUrl}");
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
            System.Console.WriteLine($"[DEBUG] Raw StartNewGame JSON: {json}");
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
                System.Console.WriteLine($"[DEBUG] Sending POST to {_baseUrl}/api/game/discover-star/{starId}");
                var resp = await _http.PostAsync($"{_baseUrl}/api/game/discover-star/{starId}", null);
                System.Console.WriteLine($"[DEBUG] DiscoverStarAsync status: {resp.StatusCode}");
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[ERROR] DiscoverStarAsync exception: {ex.Message}");
                return false;
            }
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