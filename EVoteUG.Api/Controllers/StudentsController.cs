using EVoteUG.Core.DTOs.Auth;
using EVoteUG.Core.Interfaces;
using EVoteUG.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IAuthService _authService;

    public StudentsController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new university student account.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] StudentRegisterRequestDto dto)
    {
        var result = await _authService.RegisterStudentAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Authenticate student and return student profile.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] StudentRegisterRequestDto dto)
    {
        var identifier = !string.IsNullOrWhiteSpace(dto.Email) ? dto.Email : dto.StudentId;
        var result = await _authService.StudentDirectLoginAsync(identifier, dto.Password);
        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }
}