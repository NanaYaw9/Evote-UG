using System.Security.Claims;
using EVoteUG.Core.DTOs.Election;
using EVoteUG.Core.Interfaces;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ElectionsController : ControllerBase
{
    private readonly IElectionService _electionService;

    public ElectionsController(IElectionService electionService)
    {
        _electionService = electionService;
    }

    /// <summary>
    /// Retrieve list of elections with optional status and scope filters.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<ElectionResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetElections([FromQuery] ElectionStatus? status, [FromQuery] ElectionScope? scope)
    {
        var result = await _electionService.GetElectionsAsync(status, scope);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve single election by ID with positions and candidates.
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ElectionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ElectionResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetElectionById(int id)
    {
        var result = await _electionService.GetElectionByIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new election (Electoral Commission Admins only).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<ElectionResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ElectionResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateElection([FromBody] CreateElectionRequestDto request)
    {
        var adminId = GetAdminId();
        var result = await _electionService.CreateElectionAsync(request, adminId);
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetElectionById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update existing election metadata.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<ElectionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ElectionResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateElection(int id, [FromBody] UpdateElectionRequestDto request)
    {
        var adminId = GetAdminId();
        var result = await _electionService.UpdateElectionAsync(id, request, adminId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Update election lifecycle status (Draft -> Scheduled -> Active -> Paused -> Concluded -> Certified).
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateElectionStatus(int id, [FromBody] UpdateElectionStatusDto request)
    {
        var adminId = GetAdminId();
        var result = await _electionService.UpdateElectionStatusAsync(id, request.NewStatus, adminId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete draft election.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteElection(int id)
    {
        var adminId = GetAdminId();
        var result = await _electionService.DeleteElectionAsync(id, adminId);
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
