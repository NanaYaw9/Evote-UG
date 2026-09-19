using EVoteUG.Shared.Enums;

namespace EVoteUG.Core.DTOs.Admin;

public class AdminRegisterRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.SuperAdmin;
}
