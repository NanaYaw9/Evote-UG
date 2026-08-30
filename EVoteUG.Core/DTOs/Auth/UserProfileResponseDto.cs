namespace EVoteUG.Core.DTOs.Auth;

public class UserProfileResponseDto
{
    public int Id { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? College { get; set; }
    public string? Faculty { get; set; }
    public string? Department { get; set; }
    public string? HallOfResidence { get; set; }
    public int? Level { get; set; }
    public bool IsVerified { get; set; }
}
