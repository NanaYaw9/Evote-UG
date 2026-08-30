using EVoteUG.Core.DTOs.Admin;
using EVoteUG.Core.Interfaces;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireSuperAdmin")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>
    /// Retrieve paginated immutable audit logs (SuperAdmin only).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AuditLogResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int pageIndex = 1, 
        [FromQuery] int pageSize = 20, 
        [FromQuery] AuditEventType? eventType = null)
    {
        var result = await _auditService.GetAuditLogsAsync(pageIndex, pageSize, eventType);
        return Ok(result);
    }
}
