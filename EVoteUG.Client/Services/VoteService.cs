using System.Net.Http.Json;
using EVoteUG.Shared.Models;

namespace EVoteUG.Client.Services;

public class VoteService
{
    private readonly HttpClient _http;

    public VoteService(HttpClient http)
    {
        _http = http;
    }

    public async Task<(bool Success, string Message)> CastVoteAsync(Vote vote)
    {
        var response = await _http.PostAsJsonAsync("api/votes", vote);

        if (response.IsSuccessStatusCode)
        {
            return (true, "Vote cast successfully!");
        }
        else
        {
            var errorText = await response.Content.ReadAsStringAsync();
            return (false, errorText);
        }
    }
}