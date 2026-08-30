namespace EVoteUG.Core.DTOs.Results;

public class TurnoutAnalyticsDto
{
    public int ElectionId { get; set; }
    public int TotalEligibleVoters { get; set; }
    public int TotalVotesCast { get; set; }
    public double OverallTurnoutPercentage { get; set; }
    public Dictionary<string, DemographicTurnoutItemDto> TurnoutByFaculty { get; set; } = new();
    public Dictionary<string, DemographicTurnoutItemDto> TurnoutByHall { get; set; } = new();
    public Dictionary<string, DemographicTurnoutItemDto> TurnoutByLevel { get; set; } = new();
}

public class DemographicTurnoutItemDto
{
    public int TotalEligible { get; set; }
    public int VotesCast { get; set; }
    public double TurnoutPercentage => TotalEligible > 0 
        ? Math.Round(((double)VotesCast / TotalEligible) * 100, 2) 
        : 0;
}
