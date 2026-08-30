using EVoteUG.Core.DTOs.Candidate;

namespace EVoteUG.Core.DTOs.Position;

public class PositionResponseDto
{
    public int Id { get; set; }
    public int ElectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxVotesAllowed { get; set; } = 1;
    public int OrderIndex { get; set; }
    public List<CandidateResponseDto> Candidates { get; set; } = new();
}
