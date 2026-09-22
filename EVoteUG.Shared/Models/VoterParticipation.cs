namespace EVoteUG.Shared.Models;

/// <summary>
/// Records WHO participated in an election to prevent double voting.
/// Intentionally contains NO candidate or choice information to guarantee ballot secrecy.
/// </summary>
public class VoterParticipation
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public int ElectionId { get; set; }
    public Election? Election { get; set; }
    public DateTime CastAt { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public string DeviceInfo { get; set; } = string.Empty;
}
