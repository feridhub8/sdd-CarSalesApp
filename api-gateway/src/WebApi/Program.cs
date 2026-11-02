using WebApi.Extensions.V1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.InitReverseProxy(builder.Configuration);
builder.Services.InitRateLimiting(builder.Configuration);
builder.Services.InitHealthChecks();

var app = builder.Build();

app.UseRateLimiter();

app.MapReverseProxy();
app.MapHealthCheckEndpoints();

// Without this an unknown API path would fall through to the website's catch-all route.
app.Map("/api/{**rest}", () => Results.NotFound());

app.Run();
