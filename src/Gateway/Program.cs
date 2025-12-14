using Gateway.Security;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Primitives;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddSingleton<ISsoConfigurator, NoopSsoConfigurator>();
// TODO: Wire organization-provided AddSso implementation by replacing the NoopSsoConfigurator registration above.

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.Use(async (context, next) =>
{
    if (!context.Request.Headers.TryGetValue("X-Correlation-ID", out StringValues correlationIds) || StringValues.IsNullOrEmpty(correlationIds))
    {
        var correlationId = Guid.NewGuid().ToString();
        context.Request.Headers["X-Correlation-ID"] = correlationId;
    }

    context.Response.OnStarting(() =>
    {
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var currentId))
        {
            context.Response.Headers["X-Correlation-ID"] = currentId.ToString();
        }
        return Task.CompletedTask;
    });

    await next();
});

var ssoConfigurator = app.Services.GetRequiredService<ISsoConfigurator>();
ssoConfigurator.Configure(app);

app.MapReverseProxy();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
