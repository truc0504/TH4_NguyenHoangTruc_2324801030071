using Microsoft.Extensions.DependencyInjection;

namespace AuthService;

public static class ServiceRegistration
{
    public static IServiceCollection AddAuthService(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(ServiceRegistration).Assembly));

        return services;
    }
}