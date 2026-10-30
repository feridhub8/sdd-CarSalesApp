using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApi.Persistence;

namespace WebApi.Extensions.V1;

public static class DatabaseExtensions
{
    public const string MigrateArgument = "--migrate";

    private const string ConnectionStringName = "Default";

    public static void InitDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // The whole connection string comes from Vault (ConnectionStrings:Default); appsettings has none.
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"'ConnectionStrings:{ConnectionStringName}' is missing; it is read from Vault.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseApplicationDatabase(connectionString));
    }

    // Without a connection string (design time) the provider is configured but never connects.
    public static DbContextOptionsBuilder UseApplicationDatabase(this DbContextOptionsBuilder options, string? connectionString = null)
    {
        var npgsql = connectionString is null ? options.UseNpgsql() : options.UseNpgsql(connectionString);
        return npgsql.UseSnakeCaseNamingConvention();
    }

    public static async Task MigrateDatabaseAsync(this WebApplication app, CancellationToken ct = default)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // On a new database the history table does not exist yet, and EF logs every query against it as a
        // failure before creating it. Creating it first keeps the first run's log clean.
        await context.GetService<IHistoryRepository>().CreateIfNotExistsAsync(ct);

        var pending = (await context.Database.GetPendingMigrationsAsync(ct)).ToList();
        if (pending.Count == 0)
        {
            app.Logger.LogInformation("The database is up to date.");
            return;
        }

        await context.Database.MigrateAsync(ct);
        app.Logger.LogInformation("Applied migrations: {Migrations}", string.Join(", ", pending));
    }
}
