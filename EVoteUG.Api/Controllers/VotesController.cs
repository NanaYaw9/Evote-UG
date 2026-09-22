using System.Security.Claims;
using EVoteUG.Core.DTOs.Results;
using EVoteUG.Core.DTOs.Voting;
using EVoteUG.Core.Interfaces;
using EVoteUG.Shared.Models;
using EVoteUG.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VotesController : ControllerBase
{
    private readonly IVotingService _votingService;
    private readonly IResultsService _resultsService;

    public VotesController(IVotingService votingService, IResultsService resultsService)
    {
        _votingService = votingService;
        _resultsService = resultsService;
    }

    /// <summary>
    /// Retrieve customized ballot for authenticated student (checks scope eligibility and voting status).
    /// </summary>
    [HttpGet("ballot/{electionId}")]
    [Authorize(Policy = "RequireStudent")]
    [ProducesResponseType(typeof(ApiResponse<BallotResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BallotResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBallot(int electionId)
    {
        var studentId = GetStudentId();
        var result = await _votingService.GetEligibleBallotAsync(electionId, studentId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Check whether authenticated student is eligible and/or has already voted in an election.
    /// </summary>
    [HttpGet("status/{electionId}")]
    [Authorize(Policy = "RequireStudent")]
    [ProducesResponseType(typeof(ApiResponse<VoterStatusResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckVoterStatus(int electionId)
    {
        var studentId = GetStudentId();
        var result = await _votingService.CheckVoterStatusAsync(electionId, studentId);
        return Ok(result);
    }

    /// <summary>
    /// Cast ballot with atomic secret ballot decoupling and SHA-256 digital receipt issuance.
    /// </summary>
    [HttpPost("cast")]
    [Authorize(Policy = "RequireStudent")]
    [ProducesResponseType(typeof(ApiResponse<VoteReceiptResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VoteReceiptResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CastBallot([FromBody] CastBallotRequestDto request)
    {
        var studentId = GetStudentId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();

        var result = await _votingService.CastBallotAsync(studentId, request, ipAddress, userAgent);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Retrieve all cryptographic vote receipts issued to authenticated student.
    /// </summary>
    [HttpGet("receipts")]
    [Authorize(Policy = "RequireStudent")]
    [ProducesResponseType(typeof(ApiResponse<List<VoteReceiptResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyReceipts()
    {
        var studentId = GetStudentId();
        var result = await _votingService.GetStudentReceiptsAsync(studentId);
        return Ok(result);
    }

    /// <summary>
    /// Public cryptographic ledger check: Verify authenticity of a digital receipt hash.
    /// </summary>
    [HttpGet("verify-receipt/{receiptHash}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyReceipt(string receiptHash)
    {
        var result = await _votingService.VerifyReceiptHashAsync(receiptHash);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Cast vote directly (used by client voting).
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<Vote>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Vote>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CastVote([FromBody] Vote vote)
    {
        var result = await _votingService.CastDirectVoteAsync(vote);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Retrieve candidate vote counts for a position.
    /// </summary>
    [HttpGet("results/{positionId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<PositionResultItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResults(int positionId)
    {
        var result = await _resultsService.GetPositionResultsAsync(positionId);
        return Ok(result);
    }

    private int GetStudentId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }
}
