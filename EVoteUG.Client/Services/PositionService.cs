using System.Net.Http.Json;
using EVoteUG.Shared.Models;

namespace EVoteUG.Client.Services;

public class PositionService
{
    private readonly HttpClient _http;

    public PositionService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Position>> GetPositionsByElectionAsync(int electionId)
    {
        try
        {
            var envelope = await _http.GetFromJsonAsync<EVoteUG.Shared.Responses.ApiResponse<List<Position>>>($"api/positions?electionId={electionId}");
            if (envelope?.Data != null)
                return envelope.Data;
        }
        catch { }

        try
        {
            var positions = await _http.GetFromJsonAsync<List<Position>>($"api/positions?electionId={electionId}");
            return positions ?? new List<Position>();
        }
        catch
        {
            return new List<Position>();
        }
    }

    public async Task<Position?> GetPositionWithCandidatesAsync(int positionId)
    {
        try
        {
            var envelope = await _http.GetFromJsonAsync<EVoteUG.Shared.Responses.ApiResponse<Position>>($"api/positions/{positionId}");
            if (envelope?.Data != null)
                return envelope.Data;
        }
        catch { }

        try
        {
            return await _http.GetFromJsonAsync<Position>($"api/positions/{positionId}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Success, string Message)> CreatePositionAsync(Position position)
{
    var response = await _http.PostAsJsonAsync("api/positions", position);

    if (response.IsSuccessStatusCode)
    {
        return (true, "Position created successfully!");
    }
    else
    {
        var errorText = await response.Content.ReadAsStringAsync();
        return (false, errorText);
    }
}

public async Task<(bool Success, string Message)> UpdatePositionAsync(Position position)
{
    var response = await _http.PutAsJsonAsync($"api/positions/{position.Id}", position);

    if (response.IsSuccessStatusCode)
    {
        return (true, "Position updated successfully!");
    }
    else
    {
        var errorText = await response.Content.ReadAsStringAsync();
        return (false, errorText);
    }
}
}