using System.Net.Http.Json;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Client.Services;

public class ElectionResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string ScopeName { get; set; } = string.Empty;
    public string ScopeTarget { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool AllowRealtimeResults { get; set; }
    public List<PositionResponse> Positions { get; set; } = new();
}

public class CreateElectionRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = "2026/2027";
    public ElectionScope Scope { get; set; } = ElectionScope.SRC;
    public string ScopeTarget { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool AllowRealtimeResults { get; set; } = false;
}

public class ElectionService
{
    private readonly HttpClient _http;

    public ElectionService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ElectionResponse>> GetElectionsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ElectionResponse>>>("api/elections");
        return response?.Data ?? new List<ElectionResponse>();
    }

    public async Task<ElectionResponse?> GetElectionByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<ElectionResponse>>($"api/elections/{id}");
        return response?.Data;
    }

    public async Task<(bool Success, string Message)> CreateElectionAsync(CreateElectionRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/elections", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ElectionResponse>>();

        if (response.IsSuccessStatusCode && body?.Success == true)
            return (true, "Election created successfully!");

        return (false, body?.Message ?? "Failed to create election.");
    }

    public async Task<(bool Success, string Message)> UpdateElectionStatusAsync(int id, int newStatus)
{
    var response = await _http.PatchAsJsonAsync($"api/elections/{id}/status", new { NewStatus = newStatus });

    try
    {
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        if (response.IsSuccessStatusCode && body?.Success == true)
            return (true, "Election status updated!");

        return (false, body?.Message ?? "Failed to update status.");
    }
    catch
    {
        return (false, $"Failed to update status (server error {(int)response.StatusCode}).");
    }
}

    public async Task<(bool Success, string Message)> DeleteElectionAsync(int id)
{
    var response = await _http.DeleteAsync($"api/elections/{id}");

    try
    {
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        if (response.IsSuccessStatusCode && body?.Success == true)
            return (true, "Election deleted successfully!");

        return (false, body?.Message ?? "Failed to delete election.");
    }
    catch
    {
        return (false, $"Failed to delete election (server error {(int)response.StatusCode}).");
    }
}
}