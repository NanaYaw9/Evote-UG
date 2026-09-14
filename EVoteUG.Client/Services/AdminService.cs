using System.Net.Http.Json;

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
            var admin = await response.Content.ReadFromJsonAsync<CurrentAdmin>();
            return (true, "Login successful!", admin);
        }
        else
        {
            var errorText = await response.Content.ReadAsStringAsync();
            return (false, errorText, null);
        }
    }
}