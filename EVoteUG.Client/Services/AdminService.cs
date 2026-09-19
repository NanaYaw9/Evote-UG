using System.Net.Http.Json;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Client.Services;

public class AdminLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AdminService
{
    private readonly HttpClient _http;

    public AdminService(HttpClient http)
    {
        _http = http;
    }

    public async Task<(bool Success, string Message, CurrentAdmin? Admin)> LoginAsync(string username, string password)
    {
        var request = new AdminLoginRequest { Username = username, Password = password };
        var response = await _http.PostAsJsonAsync("api/admins/login", request);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<CurrentAdmin>>();
                if (envelope?.Data != null)
                {
                    return (true, envelope.Message ?? "Login successful!", envelope.Data);
                }
            }
            catch { }

            var direct = await response.Content.ReadFromJsonAsync<CurrentAdmin>();
            return (true, "Login successful!", direct);
        }
        else
        {
            try
            {
                var errorEnvelope = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                if (errorEnvelope != null && !string.IsNullOrWhiteSpace(errorEnvelope.Message))
                {
                    var msg = errorEnvelope.Errors?.Count > 0
                        ? $"{errorEnvelope.Message} ({string.Join(", ", errorEnvelope.Errors)})"
                        : errorEnvelope.Message;
                    return (false, msg, null);
                }
            }
            catch { }

            var errorText = await response.Content.ReadAsStringAsync();
            return (false, errorText, null);
        }
    }
}