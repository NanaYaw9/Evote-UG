namespace EVoteUG.Core.DTOs.Auth;

public class StudentLoginRequestDto
{
    public string StudentId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
