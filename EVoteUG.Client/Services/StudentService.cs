using System.Net.Http.Json;

namespace EVoteUG.Client.Services;

public class LoginRequest
{
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
            var student = await response.Content.ReadFromJsonAsync<CurrentStudent>();
            return (true, "Login successful!", student);
        }
        else
        {
            var errorText = await response.Content.ReadAsStringAsync();
            return (false, errorText, null);
        }
    }
}