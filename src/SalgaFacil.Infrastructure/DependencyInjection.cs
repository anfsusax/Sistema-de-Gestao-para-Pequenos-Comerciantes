using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=salgadospro;Username=postgres;Password=";

        services.AddDbContext<SalgaFacilDbContext>(options =>
        {
            if (IsSqlite(connectionString))
                options.UseSqlite(connectionString);
            else if (IsPostgreSql(connectionString))
                options.UseNpgsql(connectionString);
            else
                options.UseSqlServer(connectionString);
        });

        return services;
    }

    private static bool IsSqlite(string connectionString) =>
        connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase);

    private static bool IsPostgreSql(string connectionString) =>
        connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("Username=", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("User ID=", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("Npgsql", StringComparison.OrdinalIgnoreCase);
}
