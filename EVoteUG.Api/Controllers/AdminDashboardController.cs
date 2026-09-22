using System.Security.Claims;
using EVoteUG.Core.DTOs.Admin;
using EVoteUG.Core.Interfaces;
using EVoteUG.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "RequireAdmin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminDashboardController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Retrieve platform-wide executive metrics (registered students, active elections, total ballots cast, approved candidates).
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardMetrics()
    {
        var result = await _adminService.GetDashboardSummaryAsync();
        return Ok(result);
    }

    /// <summary>
    /// Ingest university registrar voter roll CSV file in bulk (StudentId, FullName, Email, College, Faculty, Department, HallOfResidence, Level).
    /// </summary>
    [HttpPost("import-voters")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<VoterImportResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VoterImportResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportVoterRoll(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<VoterImportResultDto>.Fail("No CSV file was uploaded."));

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".csv" && extension != ".txt")
            return BadRequest(ApiResponse<VoterImportResultDto>.Fail("Only CSV (.csv) files are supported for voter roll importation."));

        var adminId = GetAdminId();
        using var stream = file.OpenReadStream();
        var result = await _adminService.ImportVoterRollCsvAsync(stream, adminId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    private int GetAdminId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }
}
