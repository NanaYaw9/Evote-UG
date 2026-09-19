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
            try
            {
                var envelope = await response.Content.ReadFromJsonAsync<EVoteUG.Shared.Responses.ApiResponse<Vote>>();
                return (true, envelope?.Message ?? "Vote cast successfully!");
            }
            catch
            {
                return (true, "Vote cast successfully!");
            }
        }
        else
        {
            try
            {
                var errorEnvelope = await response.Content.ReadFromJsonAsync<EVoteUG.Shared.Responses.ApiResponse<object>>();
                if (errorEnvelope != null && !string.IsNullOrWhiteSpace(errorEnvelope.Message))
                {
                    return (false, errorEnvelope.Message);
                }
            }
            catch { }

            var errorText = await response.Content.ReadAsStringAsync();
            return (false, errorText);
        }
    }
}