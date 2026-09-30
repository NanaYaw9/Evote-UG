using EVoteUG.Core.DTOs.Admin;
using EVoteUG.Core.DTOs.Auth;
using EVoteUG.Core.Interfaces;
using EVoteUG.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVoteUG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminsController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IAuthService _authService;

    public AdminsController(IAdminService adminService, IAuthService authService)
    {
        _adminService = adminService;
        _authService = authService;
    }

    /// <summary>
    /// Authenticate administrator.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AdminResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AdminResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequestDto dto)
    {
        var authResult = await _authService.AdminLoginAsync(dto);
        if (!authResult.Success)
            return Unauthorized(ApiResponse<AdminResponseDto>.Fail(authResult.Message, authResult.Errors));

        var adminDto = new AdminResponseDto
        {
            Username = authResult.Data!.Identifier,
            FullName = authResult.Data.FullName,
            Email = authResult.Data.Email,
            IsActive = true
        };

        return Ok(ApiResponse<AdminResponseDto>.Ok(adminDto, "Login successful."));
    }

    /// <summary>
    /// Register a new administrator account. Open for demo/project purposes.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AdminResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AdminResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] AdminRegisterRequestDto dto)
    {
        var result = await _adminService.RegisterAdminAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete administrator by ID. Requires existing admin authentication.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        var result = await _adminService.DeleteAdminAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Retrieve list of all administrators. Requires existing admin authentication.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<List<AdminResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdmins()
    {
        var result = await _adminService.GetAdminsAsync();
        return Ok(result);
    }
}