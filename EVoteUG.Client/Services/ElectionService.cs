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
}