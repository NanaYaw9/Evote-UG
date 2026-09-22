using System.Net.Http.Json;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Client.Services;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class StudentService
{
    private readonly HttpClient _http;

    public StudentService(HttpClient http)
    {
        _http = http;
    }

    public async Task<(bool Success, string Message, CurrentStudent? Student)> LoginAsync(string email, string password)
    {
        var request = new LoginRequest { Email = email, Password = password };
        var response = await _http.PostAsJsonAsync("api/students/login", request);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<CurrentStudent>>();
                if (envelope?.Data != null)
                {
                    return (true, envelope.Message ?? "Login successful!", envelope.Data);
                }
            }
            catch { }

            var direct = await response.Content.ReadFromJsonAsync<CurrentStudent>();
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

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/students/register", request);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                return (true, envelope?.Message ?? "Registration successful! You can now log in.");
            }
            catch
            {
                return (true, "Registration successful! You can now log in.");
            }
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
                    return (false, msg);
                }
            }
            catch { }

            var errorText = await response.Content.ReadAsStringAsync();
            return (false, errorText);
        }
    }
}