using ServiceDefaults;
using Ui.BlazorServer;
using Ui.BlazorServer.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ServiceName"] = "ui-blazor-server";
builder.AddServiceDefaults();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
