using System.Text.Json;
using EVoteUG.Core.DTOs.Admin;
using EVoteUG.Core.Interfaces;
using EVoteUG.Infrastructure.Data;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Models;
using EVoteUG.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace EVoteUG.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly EVoteUGDbContext _context;

    public AuditService(EVoteUGDbContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(
        int? adminId, 
        AuditEventType eventType, 
        string action, 
        string entityType, 
        int? entityId, 
        object? details = null, 
        string ipAddress = "")
    {
        var log = new AuditLog
        {
            AdminId = adminId,
            EventType = eventType,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            DetailsJson = details != null ? JsonSerializer.Serialize(details) : string.Empty,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<ApiResponse<PagedResult<AuditLogResponseDto>>> GetAuditLogsAsync(
        int pageIndex = 1, 
        int pageSize = 20, 
        AuditEventType? eventType = null)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.AuditLogs
            .Include(a => a.Admin)
            .AsNoTracking();

        if (eventType.HasValue)
        {
            query = query.Where(a => a.EventType == eventType.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogResponseDto
            {
                Id = a.Id,
                AdminId = a.AdminId,
                AdminUsername = a.Admin != null ? a.Admin.Username : "System/Anonymous",
                EventType = a.EventType,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                DetailsJson = a.DetailsJson,
                IpAddress = a.IpAddress,
                Timestamp = a.Timestamp
            })
            .ToListAsync();

        var pagedResult = new PagedResult<AuditLogResponseDto>
        {
            Items = items,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ApiResponse<PagedResult<AuditLogResponseDto>>.Ok(pagedResult, "Audit logs retrieved successfully.");
    }
}
