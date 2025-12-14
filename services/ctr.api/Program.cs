using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ServiceName"] = "CTR-api";
builder.AddServiceDefaults();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "CTR", status = "ok" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet($"/api/ctr/ping", () => Results.Ok(new { module = "CTR", message = "pong" }));

app.MapFallback(() => Results.NotFound());

app.Run();
