using Auth.Application.Services;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Auth.Domain.Entities;
using Auth.Domain.Factories;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Configurations;
using Auth.Infrastructure.Factories;
using Auth.Infrastructure.Repositories;
using Npgsql.Internal.Postgres;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // processing - 加入密鑰
        services.AddKey(configuration);

        // processing - 
        services.AddScoped<ICryptoService, CryptoService>();
        
        // processing - 
        services.AddScoped<IJwtService, JwtService>();

        // processing - Redis 配置
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(
            configuration.GetConnectionString("Redis") 
            ?? throw new NullReferenceException("Please add connection string for redis"))
        );
        
        // processing - Main Database 配置
        services.AddDbContext<AppDbContext>(opts =>
        {
            opts.UseNpgsql(configuration.GetConnectionString("Main"), o =>
            {
                o.MapEnum<Scope>("scope");
                o.MapEnum<OIDC>("oidc");
            });
        });
        
        // processing - 
        services.AddScoped<ICodeService, CodeService>();

        // processing - repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IAppRepository, AppRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Processing - 
        services.AddScoped<IPasswordService, PasswordService>();
        
        // description - snowflake id
        services.AddSingleton<SnowflakeID>(provider => 
            new SnowflakeID(workerId: 1, datacenterId: 1)
        );
        
        // factories 
        services.AddScoped<IUserFactory, UserFactory>();
        
        return services;
    }
}