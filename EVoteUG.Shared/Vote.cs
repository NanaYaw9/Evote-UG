namespace EVoteUG.Shared.Models;

public class Vote
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public int CandidateId { get; set; }
    public Candidate? Candidate { get; set; }

    public int PositionId { get; set; }
    public Position? Position { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}