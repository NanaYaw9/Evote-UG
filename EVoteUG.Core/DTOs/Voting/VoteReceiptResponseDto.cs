namespace EVoteUG.Core.DTOs.Voting;

public class VoteReceiptResponseDto
{
    public int ElectionId { get; set; }
    public string ElectionTitle { get; set; } = string.Empty;
    public string ReceiptHash { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = "Your vote has been cast and cryptographically recorded.";
}
