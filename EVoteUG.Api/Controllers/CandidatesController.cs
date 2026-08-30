using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVoteUG.Infrastructure.Data;
using EVoteUG.Shared.Models;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CandidatesController : ControllerBase
{
    private readonly EVoteUGDbContext _context;

    public CandidatesController(EVoteUGDbContext context)
    {
        _context = context;
    }

    // GET: api/candidates?positionId=1
    [HttpGet]
    public async Task<ActionResult<List<Candidate>>> GetCandidates([FromQuery] int? positionId)
    {
        var query = _context.Candidates.AsQueryable();

        if (positionId.HasValue)
            query = query.Where(c => c.PositionId == positionId.Value);

        var candidates = await query.ToListAsync();
        return Ok(candidates);
    }

    // GET: api/candidates/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Candidate>> GetCandidate(int id)
    {
        var candidate = await _context.Candidates.FindAsync(id);

        if (candidate == null)
            return NotFound();

        return Ok(candidate);
    }

    // POST: api/candidates
    [HttpPost]
    public async Task<ActionResult<Candidate>> CreateCandidate(Candidate candidate)
    {
        _context.Candidates.Add(candidate);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCandidate), new { id = candidate.Id }, candidate);
    }

    // PUT: api/candidates/5
[HttpPut("{id}")]
public async Task<IActionResult> UpdateCandidate(int id, Candidate candidate)
{
    if (id != candidate.Id)
        return BadRequest("ID mismatch.");

    _context.Entry(candidate).State = EntityState.Modified;

    try
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        var exists = await _context.Candidates.AnyAsync(c => c.Id == id);
        if (!exists)
            return NotFound();
        throw;
    }

    return NoContent();
}
}