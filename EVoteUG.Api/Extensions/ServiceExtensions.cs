using EVoteUG.Core.Interfaces;
using EVoteUG.Infrastructure.Services;
using EVoteUG.Infrastructure.Storage;

namespace EVoteUG.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IElectionService, ElectionService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<ICandidateService, CandidateService>();
        services.AddScoped<IVotingService, VotingService>();
        services.AddScoped<IResultsService, ResultsService>();
        services.AddSingleton<LocalFileStorageService>();

        return services;
    }
}
