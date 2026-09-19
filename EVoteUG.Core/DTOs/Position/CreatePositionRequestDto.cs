namespace EVoteUG.Core.DTOs.Position;

public class CreatePositionRequestDto
{
    public int ElectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxVotesAllowed { get; set; } = 1;
    public int OrderIndex { get; set; } = 0;
}
