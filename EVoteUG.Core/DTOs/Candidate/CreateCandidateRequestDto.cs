namespace EVoteUG.Core.DTOs.Candidate;

public class CreateCandidateRequestDto
{
    public int PositionId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string RunningMateName { get; set; } = string.Empty;
}
