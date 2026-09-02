using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVoteUG.Api.Data;
using EVoteUG.Shared.Models;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VotesController : ControllerBase
{
    private readonly EVoteUGDbContext _context;

    public VotesController(EVoteUGDbContext context)
    {
        _context = context;
    }

    // POST: api/votes
    [HttpPost]
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

    // GET: api/votes/results/{positionId}
[HttpGet("results/{positionId}")]
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
}