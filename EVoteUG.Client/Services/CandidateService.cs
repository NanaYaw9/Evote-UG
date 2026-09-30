using System.Net.Http.Json;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Client.Services;

public class CandidateResponse
{
    public int Id { get; set; }
    public int PositionId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string ManifestoUrl { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string RunningMateName { get; set; } = string.Empty;
    public string RunningMatePhotoUrl { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
}

public class CreateCandidateRequest
{
    public int PositionId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string RunningMateName { get; set; } = string.Empty;
}

public class CandidateService
{
    private readonly HttpClient _http;

    public CandidateService(HttpClient http)
    {
        _http = http;
    }

    public async Task<CandidateResponse?> GetCandidateByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<CandidateResponse>>($"api/candidates/{id}");
        return response?.Data;
    }

    public async Task<(bool Success, string Message)> CreateCandidateAsync(CreateCandidateRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/candidates", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CandidateResponse>>();

        if (response.IsSuccessStatusCode && body?.Success == true)
            return (true, "Candidate created successfully!");

        return (false, body?.Message ?? "Failed to create candidate.");
    }

    public async Task<(bool Success, string Message)> UpdateCandidateAsync(int id, CreateCandidateRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/candidates/{id}", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CandidateResponse>>();

        if (response.IsSuccessStatusCode && body?.Success == true)
            return (true, "Candidate updated successfully!");

        return (false, body?.Message ?? "Failed to update candidate.");
    }
}