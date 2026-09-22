namespace EVoteUG.Core.DTOs.Auth;

public class StudentResponseDto
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string College { get; set; } = string.Empty;
    public string Faculty { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string HallOfResidence { get; set; } = string.Empty;
    public int Level { get; set; } = 100;
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
}
