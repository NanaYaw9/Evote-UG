namespace EVoteUG.Core.DTOs.Auth;

public class AuthTokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string UserType { get; set; } = string.Empty; // "Student" or "Admin"
    public string Identifier { get; set; } = string.Empty; // StudentId or Username
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; } = false;
}
