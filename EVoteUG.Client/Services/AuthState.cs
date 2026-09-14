using System.Text.Json;
using Microsoft.JSInterop;

namespace EVoteUG.Client.Services;

public class CurrentStudent
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CurrentAdmin
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
}

public class AuthState
{
    private const string StudentStorageKey = "currentStudent";
    private const string AdminStorageKey = "currentAdmin";
    private readonly IJSRuntime _js;

    public CurrentStudent? Student { get; private set; }
    public CurrentAdmin? Admin { get; private set; }

    public event Action? OnChange;

    public AuthState(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        var studentJson = await _js.InvokeAsync<string?>("localStorage.getItem", StudentStorageKey);
        if (!string.IsNullOrEmpty(studentJson))
            Student = JsonSerializer.Deserialize<CurrentStudent>(studentJson);

        var adminJson = await _js.InvokeAsync<string?>("localStorage.getItem", AdminStorageKey);
        if (!string.IsNullOrEmpty(adminJson))
            Admin = JsonSerializer.Deserialize<CurrentAdmin>(adminJson);

        NotifyStateChanged();
    }

    public async Task LogInAsync(CurrentStudent student)
    {
        Student = student;
        var json = JsonSerializer.Serialize(student);
        await _js.InvokeVoidAsync("localStorage.setItem", StudentStorageKey, json);
        NotifyStateChanged();
    }

    public async Task LogInAdminAsync(CurrentAdmin admin)
    {
        Admin = admin;
        var json = JsonSerializer.Serialize(admin);
        await _js.InvokeVoidAsync("localStorage.setItem", AdminStorageKey, json);
        NotifyStateChanged();
    }

    public async Task LogOutAsync()
    {
        Student = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", StudentStorageKey);
        NotifyStateChanged();
    }

    public async Task LogOutAdminAsync()
    {
        Admin = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", AdminStorageKey);
        NotifyStateChanged();
    }

    public bool IsLoggedIn => Student != null;
    public bool IsAdminLoggedIn => Admin != null;

    private void NotifyStateChanged() => OnChange?.Invoke();
}