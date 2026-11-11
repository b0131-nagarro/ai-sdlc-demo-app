# Proposal: add-role-approval-limits

## Summary

Add role-based invoice approval limits so that Manager and Director approvals are constrained by amount and by the required approval structure. The change keeps the business rules in the domain layer, ensures rejected attempts do not mutate the invoice, and enforces that the second approver on large invoices must be a Director.

## Why

The current approval flow does not distinguish between roles or approval limits. This creates an inconsistency where a user can approve beyond the intended role authority and where large invoices can be approved without the required second reviewer.

## Goals

- Enforce single-approver role limits for Manager and Director users.
- Reject non-approvers and invalid invoice states without mutating the invoice.
- Require two distinct approvers for invoice amounts above 500,000, with the second approver being a Director.
- Preserve unchanged invoice state on any rejected approval attempt.

## Non-goals

- No creator-cannot-approve rule.
- No currency handling.
- No delegation or audit logging.
- No persistence changes.
- No new invoice status values.

## Design intent

The domain will own a pure approval policy and the `ApprovalService` will orchestrate validation and mutation. Controllers will map domain results to existing HTTP errors, and the UI will surface rejection messages in an alert element. Approval data will be tracked on the invoice as a list of approvers, with duplicate user-id checks performed case-insensitively.
