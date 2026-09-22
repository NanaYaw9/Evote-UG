namespace EVoteUG.Core.DTOs.Results;

public class PositionResultItemDto
{
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public int VoteCount { get; set; }
}
