using EVoteUG.Shared.Enums;

namespace EVoteUG.Shared.Models;

public class Candidate
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;   // Candidate University ID
    public string FullName { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;   // e.g. "The Visionary"
    public string Bio { get; set; } = string.Empty;
    public string ManifestoUrl { get; set; } = string.Empty; // URL to PDF manifesto
    public string PhotoUrl { get; set; } = string.Empty;     // URL to campaign portrait
    public string RunningMateName { get; set; } = string.Empty; // e.g. Vice Presidential candidate
    public string RunningMatePhotoUrl { get; set; } = string.Empty;
    public CandidateStatus Status { get; set; } = CandidateStatus.Approved;

    // Foreign key: which Position this Candidate is running for
    public int PositionId { get; set; }
    public Position? Position { get; set; }
}
