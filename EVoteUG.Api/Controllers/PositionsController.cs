using System.Security.Claims;
using EVoteUG.Core.DTOs.Position;
using EVoteUG.Core.Interfaces;
using EVoteUG.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    private readonly IPositionService _positionService;

    public PositionsController(IPositionService positionService)
    {
        _positionService = positionService;
    }

    /// <summary>
    /// Retrieve positions for a specific election.
    /// </summary>
    [HttpGet("by-election/{electionId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<PositionResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPositionsByElection(int electionId)
    {
        var result = await _positionService.GetPositionsByElectionAsync(electionId);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve position by ID.
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PositionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PositionResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPositionById(int id)
    {
        var result = await _positionService.GetPositionByIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new contestable position for an election.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<PositionResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PositionResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePosition([FromBody] CreatePositionRequestDto request)
    {
        var adminId = GetAdminId();
        var result = await _positionService.CreatePositionAsync(request, adminId);
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetPositionById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update position metadata.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<PositionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PositionResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePosition(int id, [FromBody] UpdatePositionRequestDto request)
    {
        var adminId = GetAdminId();
        var result = await _positionService.UpdatePositionAsync(id, request, adminId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete position.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeletePosition(int id)
    {
        var adminId = GetAdminId();
        var result = await _positionService.DeletePositionAsync(id, adminId);
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
