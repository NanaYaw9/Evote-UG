using EVoteUG.Shared.Enums;

namespace EVoteUG.Shared.Models;

public class Election
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = "2026/2027";
    public ElectionScope Scope { get; set; } = ElectionScope.SRC;
    public string ScopeTarget { get; set; } = string.Empty; // e.g. "Commonwealth Hall" if Scope is HallOfResidence
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ElectionStatus Status { get; set; } = ElectionStatus.Active;
    public bool IsActive { get; set; } = true;
    public bool AllowRealtimeResults { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // One Election has many Positions 
    public List<Position> Positions { get; set; } = new();
    public List<VoterParticipation> VoterParticipations { get; set; } = new();
}
