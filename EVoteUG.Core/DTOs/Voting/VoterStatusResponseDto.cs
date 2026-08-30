namespace EVoteUG.Core.DTOs.Voting;

public class VoterStatusResponseDto
{
    public int ElectionId { get; set; }
    public bool IsEligible { get; set; }
    public bool HasVoted { get; set; }
    public DateTime? CastAt { get; set; }
    public string? ReceiptHash { get; set; }
}
