namespace EVoteUG.Core.DTOs.Position;

public class UpdatePositionRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxVotesAllowed { get; set; } = 1;
    public int OrderIndex { get; set; }
}
