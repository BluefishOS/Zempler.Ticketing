using Microsoft.Extensions.DependencyInjection;
using Zempler.Ticketing.Application.Services;

namespace Zempler.Ticketing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITicketService, TicketService>();
        return services;
    }
}