using EVoteUG.Shared.Enums;

namespace EVoteUG.Core.DTOs.Results;

public class ElectionResultsResponseDto
{
    public int ElectionId { get; set; }
    public string ElectionTitle { get; set; } = string.Empty;
    public ElectionStatus Status { get; set; }
    public bool IsCertified { get; set; }
    public int TotalRegisteredVoters { get; set; }
    public int TotalBallotsCast { get; set; }
    public double OverallTurnoutPercentage => TotalRegisteredVoters > 0 
        ? Math.Round(((double)TotalBallotsCast / TotalRegisteredVoters) * 100, 2) 
        : 0;
    public List<PositionResultDto> PositionResults { get; set; } = new();
}

public class PositionResultDto
{
    public int PositionId { get; set; }
    public string PositionTitle { get; set; } = string.Empty;
    public int TotalVotesForPosition { get; set; }
    public int WinnerCandidateId { get; set; }
    public string WinnerCandidateName { get; set; } = string.Empty;
    public List<CandidateTallyDto> Tallies { get; set; } = new();
}

public class CandidateTallyDto
{
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public double PercentageShare { get; set; }
    public bool IsWinner { get; set; }
}
