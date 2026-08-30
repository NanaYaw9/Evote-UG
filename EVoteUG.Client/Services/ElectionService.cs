using System.Net.Http.Json;
using EVoteUG.Shared.Models;

namespace EVoteUG.Client.Services;

public class ElectionService
{
    private readonly HttpClient _http;

    public ElectionService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Election>> GetElectionsAsync()
    {
        var elections = await _http.GetFromJsonAsync<List<Election>>("api/elections");
        return elections ?? new List<Election>();
    }

    public async Task<(bool Success, string Message)> CreateElectionAsync(Election election)
{
    var response = await _http.PostAsJsonAsync("api/elections", election);

    if (response.IsSuccessStatusCode)
    {
        return (true, "Election created successfully!");
    }
    else
    {
        var errorText = await response.Content.ReadAsStringAsync();
        return (false, errorText);
    }
}
}