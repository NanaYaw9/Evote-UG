using EVoteUG.Core.Interfaces;
using EVoteUG.Infrastructure.Services;
using EVoteUG.Infrastructure.Storage;

namespace EVoteUG.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<LocalFileStorageService>();

        return services;
    }
}
