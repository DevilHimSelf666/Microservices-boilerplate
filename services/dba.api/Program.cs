using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ServiceName"] = "DBA-api";
builder.AddServiceDefaults();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "DBA", status = "ok" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet($"/api/dba/ping", () => Results.Ok(new { module = "DBA", message = "pong" }));

app.MapFallback(() => Results.NotFound());

app.Run();
