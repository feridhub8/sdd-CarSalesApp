using WebApi.Extensions.V1;
using WebApi.Middlewares.V1;

var builder = WebApplication.CreateBuilder(args);

await builder.InitVaultAsync();

builder.Services.InitLibraries();
builder.Services.InitApiVersioning();
builder.Services.InitKeycloak(builder.Configuration);
builder.Services.InitRepositories();
builder.Services.InitServices();
builder.Services.InitAuthentication(builder.Configuration);
builder.Services.InitSwagger();
builder.Services.InitHealthChecks();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandler>();
app.UseSwaggerInLocalAndDevelopment();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthCheckEndpoints();

app.Run();
