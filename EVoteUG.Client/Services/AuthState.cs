using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.JSInterop;
using System.Text;


namespace EVoteUG.Client.Services;

public class CurrentUser
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string UserType { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; }
}

public class AuthState
{
    private const string StorageKey = "currentUser";
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    public CurrentUser? User { get; private set; }

    public event Action? OnChange;

    public AuthState(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task InitializeAsync()
    {
        var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (!string.IsNullOrEmpty(json))
        {
            User = JsonSerializer.Deserialize<CurrentUser>(json);
            if (User != null)
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", User.Token);
        }
        NotifyStateChanged();
    }

    public async Task LogInAsync(CurrentUser user)
    {
        User = user;
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);
        var json = JsonSerializer.Serialize(user);
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        NotifyStateChanged();
    }

    public async Task LogOutAsync()
    {
        User = null;
        _http.DefaultRequestHeaders.Authorization = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        NotifyStateChanged();
    }

    public bool IsLoggedIn => User != null;
    public bool IsStudent => User?.UserType == "Student";
    public bool IsAdmin => User?.UserType == "Admin";

    private void NotifyStateChanged() => OnChange?.Invoke();

    public int? GetUserDatabaseId()
{
    if (User == null || string.IsNullOrEmpty(User.Token))
        return null;

    try
    {
        var parts = User.Token.Split('.');
        if (parts.Length < 2) return null;

        var payload = parts[1];
        payload = payload.Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        var jsonBytes = Convert.FromBase64String(payload);
        var json = Encoding.UTF8.GetString(jsonBytes);
        using var doc = JsonDocument.Parse(json);

        var idClaim = doc.RootElement.GetProperty("nameid").GetString();
        return int.TryParse(idClaim, out var id) ? id : null;
    }
    catch
    {
        return null;
    }
}
}