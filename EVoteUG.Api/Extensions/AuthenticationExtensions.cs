using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace EVoteUG.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration["JwtSettings:SecretKey"] 
                        ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                        ?? "DefaultSuperSecretKeyForEVoteUGPlatformWithAtLeast32Chars!";

        var issuer = configuration["JwtSettings:Issuer"] ?? "EVoteUG.Api";
        var audience = configuration["JwtSettings:Audience"] ?? "EVoteUG.Client";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireStudent", policy => policy.RequireRole("Student"));
            options.AddPolicy("RequireAdmin", policy => policy.RequireRole("SuperAdmin", "ElectoralOfficer"));
            options.AddPolicy("RequireSuperAdmin", policy => policy.RequireRole("SuperAdmin"));
        });

        return services;
    }
}
