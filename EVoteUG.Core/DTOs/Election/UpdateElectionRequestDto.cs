using EVoteUG.Shared.Enums;

namespace EVoteUG.Core.DTOs.Election;

public class UpdateElectionRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public ElectionScope Scope { get; set; }
    public string ScopeTarget { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool AllowRealtimeResults { get; set; }
}
