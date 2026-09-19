using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVoteUG.Infrastructure.Data;
using EVoteUG.Infrastructure.Security;
using EVoteUG.Api.DTOs;
using EVoteUG.Shared.Models;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminsController : ControllerBase
{
    private readonly EVoteUGDbContext _context;

    public AdminsController(EVoteUGDbContext context)
    {
        _context = context;
    }

    // POST: api/admins/login
    [HttpPost("login")]
    public async Task<ActionResult> Login(AdminLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Username and password are required.");

        var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Username == dto.Username.Trim());

        if (admin == null)
            return Unauthorized("Invalid username or password.");

        bool passwordValid = PasswordHasher.VerifyPassword(dto.Password, admin.PasswordHash);

        if (!passwordValid)
            return Unauthorized("Invalid username or password.");

        return Ok(new
        {
            admin.Id,
            admin.Username
        });
    }

    // POST: api/admins/register
    [HttpPost("register")]
    public async Task<ActionResult<Admin>> Register(AdminRegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Username and password are required.");

        var exists = await _context.Admins.AnyAsync(a => a.Username == dto.Username.Trim());
        if (exists)
            return BadRequest("An admin with this username already exists.");

        var username = dto.Username.Trim();
        var admin = new Admin
        {
            Username = username,
            FullName = username,
            Email = $"{username.ToLower().Replace(" ", "")}@ug.edu.gh",
            PasswordHash = PasswordHasher.HashPassword(dto.Password)
        };

        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();

        admin.PasswordHash = string.Empty;
        return Ok(admin);
    }


// DELETE: api/admins/5
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteAdmin(int id)
{
    var admin = await _context.Admins.FindAsync(id);
    if (admin == null)
        return NotFound();

    _context.Admins.Remove(admin);
    await _context.SaveChangesAsync();

    return NoContent();
}


// GET: api/admins
[HttpGet]
public async Task<ActionResult<List<object>>> GetAdmins()
{
    var admins = await _context.Admins
        .Select(a => new { a.Id, a.Username })
        .ToListAsync();

    return Ok(admins);
}
}