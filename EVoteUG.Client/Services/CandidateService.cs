using System.Net.Http.Json;
using EVoteUG.Shared.Models;

namespace EVoteUG.Client.Services;

public class CandidateService
{
    private readonly HttpClient _http;

    public CandidateService(HttpClient http)
    {
        _http = http;
    }

    public async Task<(bool Success, string Message)> CreateCandidateAsync(Candidate candidate)
    {
        var response = await _http.PostAsJsonAsync("api/candidates", candidate);

        if (response.IsSuccessStatusCode)
        {
            return (true, "Candidate created successfully!");
        }
        else
        {
            var errorText = await response.Content.ReadAsStringAsync();
            return (false, errorText);
        }
    }

    public async Task<Candidate?> GetCandidateAsync(int id)
{
    return await _http.GetFromJsonAsync<Candidate>($"api/candidates/{id}");
}

public async Task<(bool Success, string Message)> UpdateCandidateAsync(Candidate candidate)
{
    var response = await _http.PutAsJsonAsync($"api/candidates/{candidate.Id}", candidate);

    if (response.IsSuccessStatusCode)
    {
        return (true, "Candidate updated successfully!");
    }
    else
    {
        var errorText = await response.Content.ReadAsStringAsync();
        return (false, errorText);
    }
}
}