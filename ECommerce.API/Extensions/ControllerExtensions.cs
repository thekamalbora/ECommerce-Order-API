namespace ECommerce.API.Extensions;

public static class ControllerExtensions
{
    public static IServiceCollection AddControllerSetup(this IServiceCollection services)
    {
        services.AddControllers();

        return services;
    }
}