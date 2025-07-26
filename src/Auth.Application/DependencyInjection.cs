using Microsoft.Extensions.DependencyInjection;
using Shared.Mediator;

namespace Auth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // processing - 
        services.AddMediator();
        
        return services;
    }
}