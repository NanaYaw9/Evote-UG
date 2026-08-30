using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EVoteUG.Shared.Models;
using Microsoft.IdentityModel.Tokens;

namespace EVoteUG.Infrastructure.Security;

public class JwtTokenGenerator
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryInMinutes;

    public JwtTokenGenerator(string secretKey, string issuer = "EVoteUG.Api", string audience = "EVoteUG.Client", int expiryInMinutes = 120)
    {
        _secretKey = secretKey;
        _issuer = issuer;
        _audience = audience;
        _expiryInMinutes = expiryInMinutes;
    }

    public (string Token, DateTime ExpiresAt) GenerateStudentToken(Student student)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, student.Id.ToString()),
            new("studentid", student.StudentId),
            new(ClaimTypes.Name, student.FullName),
            new(ClaimTypes.Email, student.Email),
            new(ClaimTypes.Role, "Student"),
            new("faculty", student.Faculty),
            new("department", student.Department),
            new("hall", student.HallOfResidence),
            new("level", student.Level.ToString())
        };

        return GenerateToken(claims);
    }

    public (string Token, DateTime ExpiresAt) GenerateAdminToken(Admin admin)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new(ClaimTypes.Name, admin.FullName),
            new("username", admin.Username),
            new(ClaimTypes.Email, admin.Email),
            new(ClaimTypes.Role, admin.Role.ToString())
        };

        return GenerateToken(claims);
    }

    private (string Token, DateTime ExpiresAt) GenerateToken(IEnumerable<Claim> claims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(_expiryInMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiresAt);
    }
}
