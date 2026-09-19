using System.Security.Claims;
using EVoteUG.Core.DTOs.Candidate;
using EVoteUG.Core.Interfaces;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateService _candidateService;

    public CandidatesController(ICandidateService candidateService)
    {
        _candidateService = candidateService;
    }

    /// <summary>
    /// Retrieve all approved candidates running for a position.
    /// </summary>
    [HttpGet("by-position/{positionId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<CandidateResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCandidatesByPosition(int positionId)
    {
        var result = await _candidateService.GetCandidatesByPositionAsync(positionId);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve candidate by ID.
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<CandidateResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CandidateResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCandidateById(int id)
    {
        var result = await _candidateService.GetCandidateByIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Register a new candidate nomination for a position.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<CandidateResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CandidateResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCandidate([FromBody] CreateCandidateRequestDto request)
    {
        var adminId = GetAdminId();
        var result = await _candidateService.CreateCandidateAsync(request, adminId);
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetCandidateById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update candidate profile.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<CandidateResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CandidateResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCandidate(int id, [FromBody] UpdateCandidateRequestDto request)
    {
        var adminId = GetAdminId();
        var result = await _candidateService.UpdateCandidateAsync(id, request, adminId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Update candidate vetting status (Pending, Vetted, Approved, Disqualified).
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCandidateStatus(int id, [FromBody] CandidateStatusUpdateDto request)
    {
        var adminId = GetAdminId();
        var result = await _candidateService.UpdateCandidateStatusAsync(id, request.NewStatus, adminId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Upload candidate campaign portrait image (.jpg, .png).
    /// </summary>
    [HttpPost("{id}/photo")]
    [Authorize(Policy = "RequireAdmin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("No file uploaded."));

        var adminId = GetAdminId();
        using var stream = file.OpenReadStream();
        var result = await _candidateService.UploadCandidatePhotoAsync(id, stream, file.FileName, adminId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Upload candidate campaign manifesto PDF.
    /// </summary>
    [HttpPost("{id}/manifesto")]
    [Authorize(Policy = "RequireAdmin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadManifesto(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("No file uploaded."));

        var adminId = GetAdminId();
        using var stream = file.OpenReadStream();
        var result = await _candidateService.UploadCandidateManifestoAsync(id, stream, file.FileName, adminId);
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
