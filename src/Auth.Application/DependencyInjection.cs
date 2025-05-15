using Auth.Application.Commands;
using Auth.Application.Factories;
using Auth.Application.Services;
using Auth.Domain.Factories;
using Microsoft.Extensions.DependencyInjection;
using Shared.Mediator;

namespace Auth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // processing - 
        services.AddMediator();
        
        // processing - 
        services.AddScoped<IUserFactory, UserFactory>();
        
        
        return services;
    }
}