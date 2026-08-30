using EVoteUG.Shared.Enums;

namespace EVoteUG.Core.DTOs.Admin;

public class AuditLogResponseDto
{
    public int Id { get; set; }
    public int? AdminId { get; set; }
    public string AdminUsername { get; set; } = string.Empty;
    public AuditEventType EventType { get; set; }
    public string EventTypeName => EventType.ToString();
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string DetailsJson { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
