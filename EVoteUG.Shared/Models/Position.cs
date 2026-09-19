namespace EVoteUG.Shared.Models;

public class Position
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;   // e.g. "SRC President", "General Secretary"
    public string Description { get; set; } = string.Empty;
    public int MaxVotesAllowed { get; set; } = 1;       // Single choice (1) or multiple
    public int OrderIndex { get; set; } = 0;

    // Foreign key: which Election this Position belongs to
    public int ElectionId { get; set; }
    public Election? Election { get; set; }

    // One Position has many Candidates running for it
    public List<Candidate> Candidates { get; set; } = new();
}
