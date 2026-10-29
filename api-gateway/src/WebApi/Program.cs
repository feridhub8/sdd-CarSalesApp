using WebApi.Extensions.V1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.InitReverseProxy(builder.Configuration);
builder.Services.InitHealthChecks();

var app = builder.Build();

app.MapReverseProxy();
app.MapHealthCheckEndpoints();

app.Run();
