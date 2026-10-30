using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WebApi.Extensions.V1;

namespace WebApi.Persistence;

// Used by "dotnet ef migrations add". Creating a migration never connects, so it needs no connection string.
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>();
        options.UseApplicationDatabase();
        return new ApplicationDbContext(options.Options);
    }
}
