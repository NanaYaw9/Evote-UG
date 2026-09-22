using EVoteUG.Core.DTOs.Position;
using EVoteUG.Shared.Enums;

namespace EVoteUG.Core.DTOs.Election;

public class ElectionResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public ElectionScope Scope { get; set; }
    public string ScopeName => Scope.ToString();
    public string ScopeTarget { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ElectionStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public bool IsActive { get; set; }
    public bool AllowRealtimeResults { get; set; }
    public int TotalPositions { get; set; }
    public int TotalVotersParticipated { get; set; }
    public List<PositionResponseDto> Positions { get; set; } = new();
}
