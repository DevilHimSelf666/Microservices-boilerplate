using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ServiceName"] = "RST-api";
builder.AddServiceDefaults();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "RST", status = "ok" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet($"/api/rst/ping", () => Results.Ok(new { module = "RST", message = "pong" }));

app.MapFallback(() => Results.NotFound());

app.Run();
