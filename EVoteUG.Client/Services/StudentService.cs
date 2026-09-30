using System.Net.Http.Json;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Client.Services;

public class StudentRegisterRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string College { get; set; } = string.Empty;
    public string Faculty { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string HallOfResidence { get; set; } = string.Empty;
    public int Level { get; set; } = 100;
}

public class StudentLoginRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class StudentService
{
    private readonly HttpClient _http;

    public StudentService(HttpClient http)
    {
        _http = http;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(StudentRegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/students/register", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        if (response.IsSuccessStatusCode && body?.Success == true)
            return (true, "Registration successful! You can now log in.");

        return (false, body?.Message ?? "Registration failed.");
    }

    public async Task<(bool Success, string Message, CurrentUser? User)> LoginAsync(string studentId, string password)
    {
        var request = new StudentLoginRequest { StudentId = studentId, Password = password };
        var response = await _http.PostAsJsonAsync("api/auth/student-login", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CurrentUser>>();

        if (response.IsSuccessStatusCode && body?.Success == true && body.Data != null)
            return (true, "Login successful!", body.Data);

        return (false, body?.Message ?? "Login failed.", null);
    }
}