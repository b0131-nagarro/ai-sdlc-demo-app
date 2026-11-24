using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class ApprovalPolicyTests
{
    private static Invoice InvoiceWith(string id, InvoiceStatus status, decimal amount) =>
        new()
        {
            Id = id,
            CustomerName = "Test School",
            Amount = amount,
            Status = status,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

    private static void AssertInvoiceUnchanged(Invoice before, Invoice after)
    {
        Assert.Equal(before.Id, after.Id);
        Assert.Equal(before.CustomerName, after.CustomerName);
        Assert.Equal(before.Amount, after.Amount);
        Assert.Equal(before.Status, after.Status);
        Assert.Equal(before.CreatedAt, after.CreatedAt);
        Assert.Equal(before.DecidedBy, after.DecidedBy);
        Assert.Equal(before.DecidedAt, after.DecidedAt);
        Assert.Equal(before.DecisionNote, after.DecisionNote);
    }

    [Fact]
    public void T1_Manager_Approves_At_50k()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 50_000m);
        var policy = new ApprovalPolicy();

        var decision = policy.Evaluate(invoice, "alice", "Manager");

        Assert.True(decision.IsAccepted);
        Assert.Null(decision.Reason);
    }

    [Fact]
    public void T2_Manager_Above_50k_IsRejected()
    {
        var invoice = InvoiceWith("INV-2", InvoiceStatus.PendingApproval, 50_000.01m);
        var snapshot = new Invoice
        {
            Id = invoice.Id,
            CustomerName = invoice.CustomerName,
            Amount = invoice.Amount,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt,
            DecidedBy = invoice.DecidedBy,
            DecidedAt = invoice.DecidedAt,
            DecisionNote = invoice.DecisionNote
        };

        var decision = new ApprovalPolicy().Evaluate(invoice, "alice", "Manager");

        Assert.False(decision.IsAccepted);
        Assert.Equal(ApprovalReason.ExceedsApprovalLimit, decision.Reason);
        AssertInvoiceUnchanged(snapshot, invoice);
    }

    [Fact]
    public void T3_Director_Approves_At_500k()
    {
        var invoice = InvoiceWith("INV-3", InvoiceStatus.PendingApproval, 500_000m);
        var policy = new ApprovalPolicy();

        var decision = policy.Evaluate(invoice, "director1", "Director");

        Assert.True(decision.IsAccepted);
        Assert.Null(decision.Reason);
    }

    [Fact]
    public void T4_Admin_Is_Not_An_Approver()
    {
        var invoice = InvoiceWith("INV-4", InvoiceStatus.PendingApproval, 600_000m);
        var snapshot = new Invoice
        {
            Id = invoice.Id,
            CustomerName = invoice.CustomerName,
            Amount = invoice.Amount,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt,
            DecidedBy = invoice.DecidedBy,
            DecidedAt = invoice.DecidedAt,
            DecisionNote = invoice.DecisionNote
        };

        var decision = new ApprovalPolicy().Evaluate(invoice, "admin", "Admin");

        Assert.False(decision.IsAccepted);
        Assert.Equal(ApprovalReason.NotAnApprover, decision.Reason);
        AssertInvoiceUnchanged(snapshot, invoice);
    }

    [Fact]
    public void T5_Approval_Against_NonPending_Invoice_IsRejected()
    {
        var invoice = InvoiceWith("INV-5", InvoiceStatus.Draft, 600_000m);
        var snapshot = new Invoice
        {
            Id = invoice.Id,
            CustomerName = invoice.CustomerName,
            Amount = invoice.Amount,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt,
            DecidedBy = invoice.DecidedBy,
            DecidedAt = invoice.DecidedAt,
            DecisionNote = invoice.DecisionNote
        };

        var decision = new ApprovalPolicy().Evaluate(invoice, "alice", "Manager");

        Assert.False(decision.IsAccepted);
        Assert.Equal(ApprovalReason.InvalidState, decision.Reason);
        AssertInvoiceUnchanged(snapshot, invoice);
    }

    [Fact]
    public void T6_Director_Above_500k_IsRejected_Interim()
    {
        var invoice = InvoiceWith("INV-6", InvoiceStatus.PendingApproval, 500_000.01m);
        var snapshot = new Invoice
        {
            Id = invoice.Id,
            CustomerName = invoice.CustomerName,
            Amount = invoice.Amount,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt,
            DecidedBy = invoice.DecidedBy,
            DecidedAt = invoice.DecidedAt,
            DecisionNote = invoice.DecisionNote
        };

        var decision = new ApprovalPolicy().Evaluate(invoice, "director1", "Director");

        Assert.False(decision.IsAccepted);
        Assert.Equal(ApprovalReason.RequiresTwoApprovers, decision.Reason);
        AssertInvoiceUnchanged(snapshot, invoice);
    }

    [Fact]
    public void T7_Duplicate_Approvals_Are_Rejected_Case_Insensitive()
    {
        var invoice = InvoiceWith("INV-7", InvoiceStatus.PendingApproval, 100_000m);
        invoice.DecidedBy = "alice";
        var snapshot = new Invoice
        {
            Id = invoice.Id,
            CustomerName = invoice.CustomerName,
            Amount = invoice.Amount,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt,
            DecidedBy = invoice.DecidedBy,
            DecidedAt = invoice.DecidedAt,
            DecisionNote = invoice.DecisionNote
        };

        var decision = new ApprovalPolicy().Evaluate(invoice, "ALICE", "Manager");

        Assert.False(decision.IsAccepted);
        AssertInvoiceUnchanged(snapshot, invoice);
    }

    [Fact]
    public void T8_NonApprover_Role_IsRejected()
    {
        var invoice = InvoiceWith("INV-8", InvoiceStatus.PendingApproval, 600_000m);
        var snapshot = new Invoice
        {
            Id = invoice.Id,
            CustomerName = invoice.CustomerName,
            Amount = invoice.Amount,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt,
            DecidedBy = invoice.DecidedBy,
            DecidedAt = invoice.DecidedAt,
            DecisionNote = invoice.DecisionNote
        };

        var decision = new ApprovalPolicy().Evaluate(invoice, "alice", "Auditor");

        Assert.False(decision.IsAccepted);
        Assert.Equal(ApprovalReason.NotAnApprover, decision.Reason);
        AssertInvoiceUnchanged(snapshot, invoice);
    }

    [Fact]
    public void T9_SameApprover_Takes_Precedence_Over_SecondApprover_Check()
    {
        var invoice = InvoiceWith("INV-9", InvoiceStatus.PendingApproval, 600_000m);
        invoice.DecidedBy = "manager1";
        var snapshot = new Invoice
        {
            Id = invoice.Id,
            CustomerName = invoice.CustomerName,
            Amount = invoice.Amount,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt,
            DecidedBy = invoice.DecidedBy,
            DecidedAt = invoice.DecidedAt,
            DecisionNote = invoice.DecisionNote
        };

        var decision = new ApprovalPolicy().Evaluate(invoice, "MANAGER1", "Manager");

        Assert.False(decision.IsAccepted);
        AssertInvoiceUnchanged(snapshot, invoice);
    }
}
