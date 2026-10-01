using System.Net.Http.Json;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Client.Services;

public class PositionResponse
{
    public int Id { get; set; }
    public int ElectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxVotesAllowed { get; set; } = 1;
    public int OrderIndex { get; set; }
    public List<CandidateResponse> Candidates { get; set; } = new();
}

public class CreatePositionRequest
{
    public int ElectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxVotesAllowed { get; set; } = 1;
    public int OrderIndex { get; set; } = 0;
}

public class PositionService
{
    private readonly HttpClient _http;

    public PositionService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<PositionResponse>> GetPositionsByElectionAsync(int electionId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<PositionResponse>>>($"api/positions/by-election/{electionId}");
        return response?.Data ?? new List<PositionResponse>();
    }

    public async Task<PositionResponse?> GetPositionByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<PositionResponse>>($"api/positions/{id}");
        return response?.Data;
    }

    public async Task<(bool Success, string Message)> CreatePositionAsync(CreatePositionRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/positions", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PositionResponse>>();

        if (response.IsSuccessStatusCode && body?.Success == true)
            return (true, "Position created successfully!");

        return (false, body?.Message ?? "Failed to create position.");
    }

    public async Task<(bool Success, string Message)> UpdatePositionAsync(int id, CreatePositionRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/positions/{id}", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PositionResponse>>();

        if (response.IsSuccessStatusCode && body?.Success == true)
            return (true, "Position updated successfully!");

        return (false, body?.Message ?? "Failed to update position.");
    }
}