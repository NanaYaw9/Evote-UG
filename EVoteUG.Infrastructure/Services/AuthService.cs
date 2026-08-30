using EVoteUG.Core.DTOs.Auth;
using EVoteUG.Core.Interfaces;
using EVoteUG.Core.Validators;
using EVoteUG.Infrastructure.Data;
using EVoteUG.Infrastructure.Security;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Models;
using EVoteUG.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EVoteUG.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly EVoteUGDbContext _context;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly StudentLoginValidator _studentLoginValidator;
    private readonly AdminLoginValidator _adminLoginValidator;

    public AuthService(EVoteUGDbContext context, IConfiguration configuration)
    {
        _context = context;
        var secretKey = configuration["JwtSettings:SecretKey"] 
                        ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                        ?? "DefaultSuperSecretKeyForEVoteUGPlatformWithAtLeast32Chars!";

        var issuer = configuration["JwtSettings:Issuer"] ?? "EVoteUG.Api";
        var audience = configuration["JwtSettings:Audience"] ?? "EVoteUG.Client";
        var expiryMinutes = int.TryParse(configuration["JwtSettings:ExpiryInMinutes"], out var exp) ? exp : 120;

        _jwtTokenGenerator = new JwtTokenGenerator(secretKey, issuer, audience, expiryMinutes);
        _studentLoginValidator = new StudentLoginValidator();
        _adminLoginValidator = new AdminLoginValidator();
    }

    public async Task<ApiResponse<AuthTokenResponseDto>> StudentLoginAsync(StudentLoginRequestDto request)
    {
        var validation = await _studentLoginValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<AuthTokenResponseDto>.Fail(
                "Invalid login parameters.",
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentId == request.StudentId.Trim());

        if (student == null || !PasswordHasher.VerifyPassword(request.Password, student.PasswordHash))
        {
            return ApiResponse<AuthTokenResponseDto>.Fail("Invalid University Student ID or password.");
        }

        if (!student.IsActive)
        {
            return ApiResponse<AuthTokenResponseDto>.Fail("Your student account is currently deactivated. Please contact the Electoral Commission.");
        }

        var (token, expiresAt) = _jwtTokenGenerator.GenerateStudentToken(student);

        var responseDto = new AuthTokenResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserType = "Student",
            Identifier = student.StudentId,
            FullName = student.FullName,
            Email = student.Email,
            Role = "Student",
            MustChangePassword = false
        };

        return ApiResponse<AuthTokenResponseDto>.Ok(responseDto, "Student authentication successful.");
    }

    public async Task<ApiResponse<AuthTokenResponseDto>> AdminLoginAsync(AdminLoginRequestDto request)
    {
        var validation = await _adminLoginValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<AuthTokenResponseDto>.Fail(
                "Invalid login parameters.",
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Username.ToLower() == request.Username.Trim().ToLower());

        if (admin == null || !PasswordHasher.VerifyPassword(request.Password, admin.PasswordHash))
        {
            return ApiResponse<AuthTokenResponseDto>.Fail("Invalid administrator credentials.");
        }

        if (!admin.IsActive)
        {
            return ApiResponse<AuthTokenResponseDto>.Fail("Administrator account is inactive.");
        }

        admin.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var (token, expiresAt) = _jwtTokenGenerator.GenerateAdminToken(admin);

        var responseDto = new AuthTokenResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserType = "Admin",
            Identifier = admin.Username,
            FullName = admin.FullName,
            Email = admin.Email,
            Role = admin.Role.ToString(),
            MustChangePassword = admin.MustChangePassword
        };

        return ApiResponse<AuthTokenResponseDto>.Ok(responseDto, "Administrator authentication successful.");
    }

    public async Task<ApiResponse<UserProfileResponseDto>> GetUserProfileAsync(int userId, string role)
    {
        if (role.Equals("Student", StringComparison.OrdinalIgnoreCase))
        {
            var student = await _context.Students.FindAsync(userId);
            if (student == null)
                return ApiResponse<UserProfileResponseDto>.Fail("Student profile not found.");

            return ApiResponse<UserProfileResponseDto>.Ok(new UserProfileResponseDto
            {
                Id = student.Id,
                Identifier = student.StudentId,
                FullName = student.FullName,
                Email = student.Email,
                Role = "Student",
                College = student.College,
                Faculty = student.Faculty,
                Department = student.Department,
                HallOfResidence = student.HallOfResidence,
                Level = student.Level,
                IsVerified = student.IsVerified
            });
        }
        else
        {
            var admin = await _context.Admins.FindAsync(userId);
            if (admin == null)
                return ApiResponse<UserProfileResponseDto>.Fail("Administrator profile not found.");

            return ApiResponse<UserProfileResponseDto>.Ok(new UserProfileResponseDto
            {
                Id = admin.Id,
                Identifier = admin.Username,
                FullName = admin.FullName,
                Email = admin.Email,
                Role = admin.Role.ToString(),
                IsVerified = true
            });
        }
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, string role, ChangePasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return ApiResponse<bool>.Fail("New password must be at least 6 characters long.");

        if (request.NewPassword != request.ConfirmNewPassword)
            return ApiResponse<bool>.Fail("New password and confirmation password do not match.");

        if (role.Equals("Student", StringComparison.OrdinalIgnoreCase))
        {
            var student = await _context.Students.FindAsync(userId);
            if (student == null)
                return ApiResponse<bool>.Fail("Student not found.");

            if (!PasswordHasher.VerifyPassword(request.CurrentPassword, student.PasswordHash))
                return ApiResponse<bool>.Fail("Current password is incorrect.");

            student.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Password updated successfully.");
        }
        else
        {
            var admin = await _context.Admins.FindAsync(userId);
            if (admin == null)
                return ApiResponse<bool>.Fail("Administrator not found.");

            if (!PasswordHasher.VerifyPassword(request.CurrentPassword, admin.PasswordHash))
                return ApiResponse<bool>.Fail("Current password is incorrect.");

            admin.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            admin.MustChangePassword = false;
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Administrator password updated successfully.");
        }
    }
}
