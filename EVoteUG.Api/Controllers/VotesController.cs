using System.Security.Claims;
using EVoteUG.Core.DTOs.Voting;
using EVoteUG.Core.Interfaces;
using EVoteUG.Infrastructure.Data;
using EVoteUG.Shared.Models;
using EVoteUG.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VotesController : ControllerBase
{
    private readonly IVotingService _votingService;
    private readonly EVoteUGDbContext _context;

    public VotesController(IVotingService votingService, EVoteUGDbContext context)
    {
        _votingService = votingService;
        _context = context;
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
    public async Task<ActionResult<Vote>> CastVote(Vote vote)
    {
        // 1. Confirm the Student exists
        var studentExists = await _context.Students.AnyAsync(s => s.Id == vote.StudentId);
        if (!studentExists)
            return BadRequest("Student not found.");

        // 2. Confirm the Candidate exists AND belongs to the given Position
        var candidate = await _context.Candidates
            .FirstOrDefaultAsync(c => c.Id == vote.CandidateId);

        if (candidate == null)
            return BadRequest("Candidate not found.");

        if (candidate.PositionId != vote.PositionId)
            return BadRequest("This candidate does not belong to the specified position.");

        // 3. Confirm the Student hasn't already voted for this Position
        var alreadyVoted = await _context.Votes
            .AnyAsync(v => v.StudentId == vote.StudentId && v.PositionId == vote.PositionId);

        if (alreadyVoted)
            return BadRequest("You have already voted for this position.");

        // All checks passed — save the vote
        vote.Timestamp = DateTime.UtcNow;
        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();

        return Ok(vote);
    }

    /// <summary>
    /// Retrieve candidate vote counts for a position.
    /// </summary>
    [HttpGet("results/{positionId}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetResults(int positionId)
    {
        var results = await _context.Candidates
            .Where(c => c.PositionId == positionId)
            .Select(c => new
            {
                CandidateId = c.Id,
                CandidateName = c.FullName,
                VoteCount = _context.Votes.Count(v => v.CandidateId == c.Id)
            })
            .OrderByDescending(r => r.VoteCount)
            .ToListAsync();

        return Ok(results);
    }

    private int GetStudentId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }
}
