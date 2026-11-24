namespace InvoiceApp.Domain;

public enum ApprovalReason
{
    InvalidState,
    NotAnApprover,
    ExceedsApprovalLimit,
    RequiresTwoApprovers,
    SameApprover,
    SecondApproverMustBeDirector
}

public sealed record ApprovalDecision(bool IsAccepted, ApprovalReason? Reason = null, string? Message = null)
{
    public static ApprovalDecision Accepted() => new(true);

    public static ApprovalDecision Rejected(ApprovalReason reason, string message) =>
        new(false, reason, message);
}

public sealed class ApprovalRejectedException : InvalidOperationException
{
    public ApprovalRejectedException(ApprovalReason reason, string message)
        : base(message)
    {
        Reason = reason;
        ReasonCode = reason.ToString();
    }

    public ApprovalReason Reason { get; }
    public string ReasonCode { get; }
}

public class ApprovalPolicy
{
    public const decimal ManagerApprovalLimit = 50_000m;
    public const decimal DirectorApprovalLimit = 500_000m;

    public ApprovalDecision Evaluate(Invoice invoice, string approverUserId, string approverRole)
    {
        if (invoice.Status != InvoiceStatus.PendingApproval)
        {
            return ApprovalDecision.Rejected(
                ApprovalReason.InvalidState,
                $"Invoice {invoice.Id} is {invoice.Status}; only pending invoices can be approved.");
        }

        if (string.IsNullOrWhiteSpace(approverUserId))
        {
            return ApprovalDecision.Rejected(
                ApprovalReason.NotAnApprover,
                "An approver user id is required.");
        }

        if (!IsApproverRole(approverRole))
        {
            return ApprovalDecision.Rejected(
                ApprovalReason.NotAnApprover,
                $"User role '{approverRole}' cannot approve invoices.");
        }

        if (invoice.Amount > DirectorApprovalLimit)
        {
            return ApprovalDecision.Rejected(
                ApprovalReason.RequiresTwoApprovers,
                "Invoices above 500,000 require two approvers.");
        }

        if (invoice.Amount > ManagerApprovalLimit &&
            string.Equals(approverRole, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            return ApprovalDecision.Rejected(
                ApprovalReason.ExceedsApprovalLimit,
                "Manager approval limit exceeded.");
        }

        return ApprovalDecision.Accepted();
    }

    public static bool IsApproverRole(string? role) =>
        string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role, "Director", StringComparison.OrdinalIgnoreCase);
}
