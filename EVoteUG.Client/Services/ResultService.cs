using System.Net.Http.Json;

namespace EVoteUG.Client.Services;

public class ResultItem
{
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public int VoteCount { get; set; }
}

public class ResultService
{
    private readonly HttpClient _http;

    public ResultService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ResultItem>> GetResultsAsync(int positionId)
    {
        try
        {
            var envelope = await _http.GetFromJsonAsync<EVoteUG.Shared.Responses.ApiResponse<List<ResultItem>>>($"api/votes/results/{positionId}");
            if (envelope?.Data != null)
                return envelope.Data;
        }
        catch { }

        try
        {
            var results = await _http.GetFromJsonAsync<List<ResultItem>>($"api/votes/results/{positionId}");
            return results ?? new List<ResultItem>();
        }
        catch
        {
            return new List<ResultItem>();
        }
    }
}