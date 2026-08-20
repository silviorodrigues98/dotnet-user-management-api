using DotnetUserManagementApi.Application.Abstractions;
using DotnetUserManagementApi.Infrastructure.Persistence;
using DotnetUserManagementApi.Infrastructure.Persistence.Repositories;
using DotnetUserManagementApi.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace DotnetUserManagementApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration.GetConnectionString("Database");
        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.Equals(databaseProvider, "Postgres", StringComparison.OrdinalIgnoreCase))
            {
                // D-01/D-03: PostgreSQL (Docker) — provider Npgsql
                options.UseNpgsql(configuration.GetConnectionString("Default"));
            }
            else
            {
                // D-04: default local — SQLite zero dependências
                options.UseSqlite(configuration.GetConnectionString("Default"));
            }
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}