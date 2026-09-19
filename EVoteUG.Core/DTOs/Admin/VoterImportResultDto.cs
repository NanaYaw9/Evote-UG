namespace EVoteUG.Core.DTOs.Admin;

public class VoterImportResultDto
{
    public int TotalRecordsProcessed { get; set; }
    public int TotalImported { get; set; }
    public int TotalSkipped { get; set; }
    public List<string> Errors { get; set; } = new();
}
