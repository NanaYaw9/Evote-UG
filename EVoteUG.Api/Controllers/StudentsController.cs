using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVoteUG.Infrastructure.Data;
using EVoteUG.Infrastructure.Security;
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
        if (string.IsNullOrWhiteSpace(dto.StudentId) ||
            string.IsNullOrWhiteSpace(dto.FullName) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("All fields (Student ID, Full Name, Email, Password) are required.");
        }

        var emailNormalized = dto.Email.Trim().ToLower();
        var studentIdTrimmed = dto.StudentId.Trim();

        var emailExists = await _context.Students.AnyAsync(s => s.Email.ToLower() == emailNormalized);
        if (emailExists)
            return BadRequest("An account with this email already exists.");

        var studentIdExists = await _context.Students.AnyAsync(s => s.StudentId == studentIdTrimmed);
        if (studentIdExists)
            return BadRequest("An account with this Student ID already exists.");

        var student = new Student
        {
            StudentId = studentIdTrimmed,
            FullName = dto.FullName.Trim(),
            Email = emailNormalized,
            PasswordHash = PasswordHasher.HashPassword(dto.Password)
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
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Email and password are required.");

        var identifier = dto.Email.Trim();
        var student = await _context.Students.FirstOrDefaultAsync(s => 
            s.Email.ToLower() == identifier.ToLower() || s.StudentId == identifier);

        if (student == null)
            return Unauthorized("Invalid email or password.");

        bool passwordValid = PasswordHasher.VerifyPassword(dto.Password, student.PasswordHash);

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