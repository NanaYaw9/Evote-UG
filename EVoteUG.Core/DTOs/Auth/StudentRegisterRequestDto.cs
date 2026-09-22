namespace EVoteUG.Core.DTOs.Auth;

public class StudentRegisterRequestDto
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
