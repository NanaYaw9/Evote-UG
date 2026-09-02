using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVoteUG.Api.Data;
using EVoteUG.Api.DTOs;
using EVoteUG.Shared.Models;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly EVoteUGDbContext _context;

    public StudentsController(EVoteUGDbContext context)
    {
        _context = context;
    }

    // POST: api/students/register
    [HttpPost("register")]
    public async Task<ActionResult<Student>> Register(RegisterDto dto)
    {
        var emailExists = await _context.Students.AnyAsync(s => s.Email == dto.Email);
        if (emailExists)
            return BadRequest("An account with this email already exists.");

        var studentIdExists = await _context.Students.AnyAsync(s => s.StudentId == dto.StudentId);
        if (studentIdExists)
            return BadRequest("An account with this Student ID already exists.");

        var student = new Student
        {
            StudentId = dto.StudentId,
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        student.PasswordHash = string.Empty;
        return Ok(student);
    }

    // POST: api/students/login
    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto dto)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == dto.Email);

        if (student == null)
            return Unauthorized("Invalid email or password.");

        bool passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, student.PasswordHash);

        if (!passwordValid)
            return Unauthorized("Invalid email or password.");

        return Ok(new
        {
            student.Id,
            student.StudentId,
            student.FullName,
            student.Email
        });
    }
}