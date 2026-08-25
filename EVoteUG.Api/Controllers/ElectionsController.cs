using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVoteUG.Api.Data;
using EVoteUG.Shared.Models;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ElectionsController : ControllerBase
{
    private readonly EVoteUGDbContext _context;

    public ElectionsController(EVoteUGDbContext context)
    {
        _context = context;
    }

    // GET: api/elections
    [HttpGet]
    public async Task<ActionResult<List<Election>>> GetElections()
    {
        var elections = await _context.Elections.ToListAsync();
        return Ok(elections);
    }

    // GET: api/elections/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Election>> GetElection(int id)
    {
        var election = await _context.Elections.FindAsync(id);
        if (election == null)
            return NotFound();

        return Ok(election);
    }

    // POST: api/elections
    [HttpPost]
    public async Task<ActionResult<Election>> CreateElection(Election election)
    {
        _context.Elections.Add(election);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetElection), new { id = election.Id }, election);
    }
}