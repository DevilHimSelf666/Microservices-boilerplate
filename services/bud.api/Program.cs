using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ServiceName"] = "BUD-api";
builder.AddServiceDefaults();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "BUD", status = "ok" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet($"/api/bud/ping", () => Results.Ok(new { module = "BUD", message = "pong" }));

app.MapFallback(() => Results.NotFound());

app.Run();
