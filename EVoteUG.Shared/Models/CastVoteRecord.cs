namespace EVoteUG.Shared.Models;

/// <summary>
/// Records WHAT vote selection was cast.
/// Intentionally contains NO student identifier to mathematically preserve ballot secrecy.
/// </summary>
public class CastVoteRecord
{
    public int Id { get; set; }
    public int ElectionId { get; set; }
    public int PositionId { get; set; }
    public Position? Position { get; set; }
    public int CandidateId { get; set; }
    public Candidate? Candidate { get; set; }
    public DateTime CastTimestamp { get; set; } = DateTime.UtcNow;
    public string BallotBatchId { get; set; } = string.Empty;
}
