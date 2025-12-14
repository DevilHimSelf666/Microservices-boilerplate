using System.Collections.Concurrent;

namespace Elsa.Server;

public class BudgetTwoLevelApprovalWorkflow
{
    private readonly ConcurrentDictionary<Guid, BudgetApprovalInstance> _instances = new();

    public BudgetApprovalInstance Start(BudgetApprovalRequest request)
    {
        var instance = new BudgetApprovalInstance
        {
            Id = Guid.NewGuid(),
            Service = string.IsNullOrWhiteSpace(request.Service) ? "BUD" : request.Service,
            RequestedBy = request.RequestedBy,
            CostCenter = request.CostCenter,
            Amount = request.Amount,
            Description = request.Description ?? string.Empty
        };

        instance.Steps.Add(new ApprovalStep
        {
            Role = "department-manager",
            DisplayName = "Department manager approves staffing and scope coverage",
            Requirements = "Validates demand fits within team capacity and strategic priorities"
        });

        instance.Steps.Add(new ApprovalStep
        {
            Role = "finance-controller",
            DisplayName = "Finance controller validates funding and GL alignment",
            Requirements = "Confirms budget line, cost center availability, and posting rules"
        });

        _instances[instance.Id] = instance;
        return instance;
    }

    public BudgetApprovalInstance? Get(Guid id)
    {
        _instances.TryGetValue(id, out var instance);
        return instance;
    }

    public DecisionResult ApplyDecision(Guid id, ApprovalAction action)
    {
        if (!_instances.TryGetValue(id, out var instance))
        {
            return DecisionResult.Missing();
        }

        var pendingStep = instance.CurrentStep;
        if (pendingStep is null)
        {
            return DecisionResult.Validation(instance, "Workflow already completed.");
        }

        if (!pendingStep.Role.Equals(action.Role, StringComparison.OrdinalIgnoreCase))
        {
            return DecisionResult.Validation(instance, $"Awaiting decision from {pendingStep.Role} before role {action.Role} can act.");
        }

        pendingStep.Complete(action.Approver, action.Approve, action.Comments);

        if (pendingStep.Outcome == ApprovalOutcome.Rejected)
        {
            instance.Status = WorkflowStatus.Rejected;
        }
        else if (instance.Steps.All(step => step.Outcome == ApprovalOutcome.Approved))
        {
            instance.Status = WorkflowStatus.Approved;
        }

        return DecisionResult.Success(instance);
    }
}

public record BudgetApprovalRequest
{
    public string Service { get; init; } = "BUD";
    public string RequestedBy { get; init; } = string.Empty;
    public string CostCenter { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}

public record ApprovalAction
{
    public string Role { get; init; } = string.Empty;
    public string Approver { get; init; } = string.Empty;
    public bool Approve { get; init; }
    public string? Comments { get; init; }
}

public class BudgetApprovalInstance
{
    public Guid Id { get; init; }
    public string Service { get; init; } = "BUD";
    public string RequestedBy { get; init; } = string.Empty;
    public string CostCenter { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Description { get; init; } = string.Empty;
    public WorkflowStatus Status { get; set; } = WorkflowStatus.WaitingForApproval;
    public List<ApprovalStep> Steps { get; } = new();
    public ApprovalStep? CurrentStep => Steps.FirstOrDefault(step => step.Outcome == ApprovalOutcome.Pending);
}

public class ApprovalStep
{
    public string Role { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Requirements { get; init; } = string.Empty;
    public string? Approver { get; private set; }
    public string? Comments { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public ApprovalOutcome Outcome { get; private set; } = ApprovalOutcome.Pending;

    public void Complete(string approver, bool approve, string? comments)
    {
        if (Outcome != ApprovalOutcome.Pending)
        {
            return;
        }

        Approver = approver;
        Comments = comments;
        CompletedAt = DateTimeOffset.UtcNow;
        Outcome = approve ? ApprovalOutcome.Approved : ApprovalOutcome.Rejected;
    }
}

public enum WorkflowStatus
{
    WaitingForApproval,
    Approved,
    Rejected
}

public enum ApprovalOutcome
{
    Pending,
    Approved,
    Rejected
}

public record DecisionResult
{
    private DecisionResult(BudgetApprovalInstance? instance, bool notFound, string? validationMessage)
    {
        Instance = instance;
        NotFound = notFound;
        ValidationMessage = validationMessage;
    }

    public BudgetApprovalInstance? Instance { get; }
    public bool NotFound { get; }
    public string? ValidationMessage { get; }

    public static DecisionResult Success(BudgetApprovalInstance instance) => new(instance, false, null);

    public static DecisionResult Missing() => new(null, true, null);

    public static DecisionResult Validation(BudgetApprovalInstance instance, string message) => new(instance, false, message);
}
