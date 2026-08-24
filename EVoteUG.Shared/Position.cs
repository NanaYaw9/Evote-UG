namespace EVoteUG.Shared.Models;

public class Position
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;   // e.g. "President", "General Secretary"

    // Foreign key: which Election this Position belongs to
    public int ElectionId { get; set; }
    public Election? Election { get; set; }

    // One Position has many Candidates running for it
    public List<Candidate> Candidates { get; set; } = new();
}