namespace EVoteUG.Shared.Models;

public class Candidate
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;

    // Foreign key: which Position this Candidate is running for
    public int PositionId { get; set; }
    public Position? Position { get; set; }

    // One Candidate can receive many Votes
    public List<Vote> Votes { get; set; } = new();
}