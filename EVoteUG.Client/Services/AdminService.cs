using System.Net.Http.Json;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Client.Services;

public class AdminLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AdminRegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; } = 3; // SuperAdmin
}

public class AdminService
{
    private readonly HttpClient _http;

    public AdminService(HttpClient http)
    {
        _http = http;
    }

    public async Task<(bool Success, string Message, CurrentUser? User)> LoginAsync(string username, string password)
    {
        var request = new AdminLoginRequest { Username = username, Password = password };
        var response = await _http.PostAsJsonAsync("api/auth/admin-login", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CurrentUser>>();

        if (response.IsSuccessStatusCode && body?.Success == true && body.Data != null)
            return (true, "Login successful!", body.Data);

        return (false, body?.Message ?? "Login failed.", null);
    }

    public async Task<(bool Success, string Message)> RegisterAsync(AdminRegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/admins/register", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        if (response.IsSuccessStatusCode && body?.Success == true)
            return (true, "Admin registered successfully!");

        return (false, body?.Message ?? "Registration failed.");
    }
}