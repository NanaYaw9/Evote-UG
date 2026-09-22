using EVoteUG.Shared.Enums;

namespace EVoteUG.Shared.Models;

/// <summary>
/// Immutable audit log tracking administrative and electoral actions.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }
    public int? AdminId { get; set; }
    public Admin? Admin { get; set; }
    public AuditEventType EventType { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty; // e.g. "Election", "Candidate"
    public int? EntityId { get; set; }
    public string DetailsJson { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
