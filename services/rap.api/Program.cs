using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ServiceName"] = "RAP-api";
builder.AddServiceDefaults();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "RAP", status = "ok" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet($"/api/rap/ping", () => Results.Ok(new { module = "RAP", message = "pong" }));

app.MapFallback(() => Results.NotFound());

app.Run();
