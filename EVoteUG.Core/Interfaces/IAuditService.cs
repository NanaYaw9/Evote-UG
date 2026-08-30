using EVoteUG.Core.DTOs.Admin;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Core.Interfaces;

public interface IAuditService
{
    Task LogActionAsync(int? adminId, AuditEventType eventType, string action, string entityType, int? entityId, object? details = null, string ipAddress = "");
    Task<ApiResponse<PagedResult<AuditLogResponseDto>>> GetAuditLogsAsync(int pageIndex = 1, int pageSize = 20, AuditEventType? eventType = null);
}
