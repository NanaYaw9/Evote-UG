using System.Security.Claims;
using EVoteUG.Core.DTOs.Results;
using EVoteUG.Core.Interfaces;
using EVoteUG.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly IResultsService _resultsService;

    public ResultsController(IResultsService resultsService)
    {
        _resultsService = resultsService;
    }

    /// <summary>
    /// Retrieve election results and candidate tallies (embargo enforced for students while election is active unless real-time results are enabled).
    /// </summary>
    [HttpGet("{electionId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ElectionResultsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ElectionResultsResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetResults(int electionId)
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        var isPrivileged = roleClaim == "SuperAdmin" || roleClaim == "ElectoralOfficer";

        var result = await _resultsService.GetElectionResultsAsync(electionId, isPrivileged);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Retrieve demographic turnout analytics across Faculty, Hall of Residence, and Level (Electoral Commission only).
    /// </summary>
    [HttpGet("{electionId}/analytics")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<TurnoutAnalyticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TurnoutAnalyticsDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTurnoutAnalytics(int electionId)
    {
        var result = await _resultsService.GetTurnoutAnalyticsAsync(electionId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
