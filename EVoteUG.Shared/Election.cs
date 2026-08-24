namespace EVoteUG.Shared.Models;

public class Election
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    // One Election has many Positions 
    public List<Position> Positions { get; set; } = new();
}