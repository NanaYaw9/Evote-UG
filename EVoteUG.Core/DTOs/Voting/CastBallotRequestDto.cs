namespace EVoteUG.Core.DTOs.Voting;

public class CastBallotRequestDto
{
    public int ElectionId { get; set; }
    public List<PositionVoteSelectionDto> Selections { get; set; } = new();
}

public class PositionVoteSelectionDto
{
    public int PositionId { get; set; }
    public int CandidateId { get; set; }
}
