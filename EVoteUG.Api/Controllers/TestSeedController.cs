using Microsoft.AspNetCore.Mvc;
using EVoteUG.Infrastructure.Data;
using EVoteUG.Shared.Models;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestSeedController : ControllerBase
{
    private readonly EVoteUGDbContext _context;

    public TestSeedController(EVoteUGDbContext context)
    {
        _context = context;
    }

    // POST: api/testseed/student
    [HttpPost("student")]
    public async Task<ActionResult<Student>> SeedStudent()
    {
        var student = new Student
        {
            StudentId = "22012345",
            FullName = "Test Student",
            Email = "test@student.ug.edu.gh",
            PasswordHash = "temporary-placeholder"
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return Ok(student);
    }
}