using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ServiceName"] = "elsa-server";
builder.AddServiceDefaults();

// TODO: Replace with real Elsa server endpoints once the Elsa package is available.

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "elsa", status = "ok" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/workflow/ping", () => Results.Ok(new { service = "elsa", message = "pong" }));

app.Run();
