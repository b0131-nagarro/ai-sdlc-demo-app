# Design: add-role-approval-limits

## Layering

- The domain owns the approval policy as a pure decision component. It receives the invoice, the acting user, and the requested approval context, and it returns either an accepted decision or a rejection reason.
- The `ApprovalService` orchestrates the flow: it checks invoice state, calls the policy, and only mutates the invoice after the policy accepts the change.
- Controllers remain thin and map domain results to HTTP responses using the existing error-handling pattern. They do not contain business rules.
- The UI displays rejection messages in an alert element and does not own the policy logic.

## Data change

- The invoice model must carry an `Approvals` list so that approval events are recorded per invoice.
- Each approval entry identifies the user who approved; the list is compared by user id, case-insensitively, to prevent duplicate approval counting.
- For amounts above 500,000, the first valid approval keeps the invoice Pending and records one approval. The second valid director approval completes the invoice.
- Rejected attempts leave the invoice object unchanged, including state, approval list, and timestamps.

## Error mapping

- InvalidState maps to the existing not-pending approval behaviour.
- NotAnApprover maps to authorization-style rejection when the acting user lacks an approver role.
- ExceedsApprovalLimit maps to the single-approver role limit failure on the amount-based path.
- SameApprover maps to duplicate approval attempts from the same user.
- SecondApproverMustBeDirector maps to a non-director attempt on the second approval step.

## Rejection precedence

The policy must evaluate reasons in the following order and stop at the first one that applies:

1. InvalidState
2. NotAnApprover
3. ExceedsApprovalLimit
4. SameApprover
5. SecondApproverMustBeDirector

This ordering ensures deterministic behavior and stable tests regardless of which multiple conditions are true.
