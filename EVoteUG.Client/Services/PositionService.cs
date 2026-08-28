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
        var positions = await _http.GetFromJsonAsync<List<Position>>($"api/positions?electionId={electionId}");
        return positions ?? new List<Position>();
    }

    public async Task<Position?> GetPositionWithCandidatesAsync(int positionId)
    {
        return await _http.GetFromJsonAsync<Position>($"api/positions/{positionId}");
    }
}