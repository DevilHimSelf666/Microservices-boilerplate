using Elsa.Server;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ServiceName"] = "elsa-server";
builder.AddServiceDefaults();

builder.Services.AddSingleton<BudgetTwoLevelApprovalWorkflow>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "elsa", status = "ok" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/workflow/ping", () => Results.Ok(new { service = "elsa", message = "pong" }));

app.MapPost("/workflow/bud/expense-approvals", (BudgetTwoLevelApprovalWorkflow workflow, BudgetApprovalRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.RequestedBy) || string.IsNullOrWhiteSpace(request.CostCenter) || request.Amount <= 0)
    {
        return Results.BadRequest(new { message = "RequestedBy, CostCenter, and positive Amount are required." });
    }

    var instance = workflow.Start(request);
    return Results.Created($"/workflow/bud/expense-approvals/{instance.Id}", instance);
});

app.MapPost("/workflow/bud/expense-approvals/{id:guid}/decisions", (Guid id, BudgetTwoLevelApprovalWorkflow workflow, ApprovalAction action) =>
{
    if (string.IsNullOrWhiteSpace(action.Role) || string.IsNullOrWhiteSpace(action.Approver))
    {
        return Results.BadRequest(new { message = "Role and Approver are required." });
    }

    var result = workflow.ApplyDecision(id, action);
    if (result.NotFound)
    {
        return Results.NotFound(new { message = "Workflow instance not found." });
    }

    if (result.ValidationMessage is not null)
    {
        return Results.BadRequest(new { message = result.ValidationMessage, instance = result.Instance });
    }

    return Results.Ok(result.Instance);
});

app.MapGet("/workflow/bud/expense-approvals/{id:guid}", (Guid id, BudgetTwoLevelApprovalWorkflow workflow) =>
{
    var instance = workflow.Get(id);
    return instance is null ? Results.NotFound(new { message = "Workflow instance not found." }) : Results.Ok(instance);
});

app.Run();
