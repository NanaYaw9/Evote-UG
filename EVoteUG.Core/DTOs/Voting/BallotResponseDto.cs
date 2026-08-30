namespace EVoteUG.Core.DTOs.Voting;

public class BallotResponseDto
{
    public int ElectionId { get; set; }
    public string ElectionTitle { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool HasVoted { get; set; }
    public List<PositionBallotItemDto> Positions { get; set; } = new();
}

public class PositionBallotItemDto
{
    public int PositionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxVotesAllowed { get; set; } = 1;
    public List<CandidateBallotItemDto> Candidates { get; set; } = new();
}

public class CandidateBallotItemDto
{
    public int CandidateId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string ManifestoUrl { get; set; } = string.Empty;
    public string RunningMateName { get; set; } = string.Empty;
    public string RunningMatePhotoUrl { get; set; } = string.Empty;
}
