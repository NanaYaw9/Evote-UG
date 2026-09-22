namespace EVoteUG.Core.DTOs.Admin;

public class DashboardSummaryDto
{
    public int TotalStudentsRegistered { get; set; }
    public int TotalElections { get; set; }
    public int ActiveElections { get; set; }
    public int TotalBallotsCastAllTime { get; set; }
    public int TotalApprovedCandidates { get; set; }
}
