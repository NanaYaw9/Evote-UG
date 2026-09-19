using EVoteUG.Shared.Enums;

namespace EVoteUG.Core.DTOs.Candidate;

public class CandidateResponseDto
{
    public int Id { get; set; }
    public int PositionId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string ManifestoUrl { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string RunningMateName { get; set; } = string.Empty;
    public string RunningMatePhotoUrl { get; set; } = string.Empty;
    public CandidateStatus Status { get; set; }
    public string StatusName => Status.ToString();
}
