using WebApi.Extensions.V1;
using WebApi.Middlewares.V1;

var builder = WebApplication.CreateBuilder(args);

await builder.InitSecretsAsync();

builder.Services.InitLibraries();
builder.Services.InitApiVersioning();
builder.Services.InitSwagger();
builder.Services.InitHealthChecks();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwaggerInLocalAndDevelopment();

app.MapControllers();
app.MapHealthCheckEndpoints();

app.Run();
