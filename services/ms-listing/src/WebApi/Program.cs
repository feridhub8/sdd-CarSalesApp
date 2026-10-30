using WebApi.Extensions.V1;
using WebApi.Middlewares.V1;

var builder = WebApplication.CreateBuilder(args);

await builder.InitVaultAsync();

builder.Services.InitLibraries();
builder.Services.InitDatabase(builder.Configuration);
builder.Services.InitRepositories();
builder.Services.InitServices();
builder.Services.InitApiVersioning();
builder.Services.InitAuthentication(builder.Configuration);
builder.Services.InitSwagger();
builder.Services.InitHealthChecks();

var app = builder.Build();

// One-shot run (the ms-listing-migrate container, or "dotnet run -- --migrate"): apply migrations and exit.
if (args.Contains(DatabaseExtensions.MigrateArgument))
{
    await app.MigrateDatabaseAsync();
    return;
}

app.UseMiddleware<GlobalExceptionHandler>();
app.UseSwaggerInLocalAndDevelopment();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthCheckEndpoints();

app.Run();
