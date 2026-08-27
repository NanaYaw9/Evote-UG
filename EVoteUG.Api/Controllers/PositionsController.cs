using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVoteUG.Api.Data;
using EVoteUG.Shared.Models;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    private readonly EVoteUGDbContext _context;

    public PositionsController(EVoteUGDbContext context)
    {
        _context = context;
    }

    // GET: api/positions?electionId=1
    [HttpGet]
    public async Task<ActionResult<List<Position>>> GetPositions([FromQuery] int? electionId)
    {
        var query = _context.Positions.AsQueryable();

        if (electionId.HasValue)
            query = query.Where(p => p.ElectionId == electionId.Value);

        var positions = await query.ToListAsync();
        return Ok(positions);
    }

    // GET: api/positions/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Position>> GetPosition(int id)
    {
        var position = await _context.Positions
            .Include(p => p.Candidates)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (position == null)
            return NotFound();

        return Ok(position);
    }

    // POST: api/positions
    [HttpPost]
    public async Task<ActionResult<Position>> CreatePosition(Position position)
    {
        _context.Positions.Add(position);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPosition), new { id = position.Id }, position);
    }
}