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

public class AuthState
{
    private const string StorageKey = "currentStudent";
    private readonly IJSRuntime _js;

    public CurrentStudent? Student { get; private set; }

    public event Action? OnChange;

    public AuthState(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);

        if (!string.IsNullOrEmpty(json))
        {
            Student = JsonSerializer.Deserialize<CurrentStudent>(json);
            NotifyStateChanged();
        }
    }

    public async Task LogInAsync(CurrentStudent student)
    {
        Student = student;
        var json = JsonSerializer.Serialize(student);
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        NotifyStateChanged();
    }

    public async Task LogOutAsync()
    {
        Student = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        NotifyStateChanged();
    }

    public bool IsLoggedIn => Student != null;

    private void NotifyStateChanged() => OnChange?.Invoke();
}