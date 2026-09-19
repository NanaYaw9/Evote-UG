using EVoteUG.Core.DTOs.Auth;
using EVoteUG.Infrastructure.Security;
using EVoteUG.Infrastructure.Services;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Models;
using EVoteUG.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EVoteUG.Tests.Services;

public class AuthServiceTests
{
    private readonly IConfiguration _configuration;

    public AuthServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"JwtSettings:SecretKey", "TestSecretKeyForUnitTestingWithAtLeast32CharsLength123!"},
            {"JwtSettings:Issuer", "EVoteUG.Api"},
            {"JwtSettings:Audience", "EVoteUG.Client"},
            {"JwtSettings:ExpiryInMinutes", "60"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task StudentLogin_WithValidCredentials_ReturnsTokenAndClaims()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var rawPassword = "StudentPassword123";
        var student = new Student
        {
            StudentId = "10987654",
            FullName = "Kwame Mensah",
            Email = "kmensah@st.ug.edu.gh",
            PasswordHash = PasswordHasher.HashPassword(rawPassword),
            Faculty = "Faculty of Science",
            Department = "Computer Science",
            HallOfResidence = "Commonwealth Hall",
            Level = 300,
            IsVerified = true,
            IsActive = true
        };
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var authService = new AuthService(context, _configuration);

        // Act
        var result = await authService.StudentLoginAsync(new StudentLoginRequestDto
        {
            StudentId = "10987654",
            Password = rawPassword
        });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data.Token);
        Assert.Equal("Student", result.Data.UserType);
        Assert.Equal("10987654", result.Data.Identifier);
    }

    [Fact]
    public async Task StudentLogin_WithIncorrectPassword_ReturnsFailure()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var student = new Student
        {
            StudentId = "10987654",
            FullName = "Kwame Mensah",
            Email = "kmensah@st.ug.edu.gh",
            PasswordHash = PasswordHasher.HashPassword("CorrectPassword123"),
            IsVerified = true,
            IsActive = true
        };
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var authService = new AuthService(context, _configuration);

        // Act
        var result = await authService.StudentLoginAsync(new StudentLoginRequestDto
        {
            StudentId = "10987654",
            Password = "WrongPassword999"
        });

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Contains("Invalid University Student ID or password", result.Message);
    }

    [Fact]
    public async Task AdminLogin_WithValidCredentials_ReturnsAdminToken()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var rawPassword = "AdminSecretPassword123!";
        var admin = new Admin
        {
            Username = "ec_superadmin",
            FullName = "Chief Electoral Commissioner",
            Email = "commissioner@ug.edu.gh",
            PasswordHash = PasswordHasher.HashPassword(rawPassword),
            Role = UserRole.SuperAdmin,
            IsActive = true
        };
        context.Admins.Add(admin);
        await context.SaveChangesAsync();

        var authService = new AuthService(context, _configuration);

        // Act
        var result = await authService.AdminLoginAsync(new AdminLoginRequestDto
        {
            Username = "ec_superadmin",
            Password = rawPassword
        });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Admin", result.Data.UserType);
        Assert.Equal(UserRole.SuperAdmin.ToString(), result.Data.Role);
    }
}
