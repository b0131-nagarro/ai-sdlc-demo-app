---
name: approval-rules
description: Approval-limit boundaries, precedence and pitfalls. Use when coding, testing or reviewing.
---
# Approval rules: working notes
The rules themselves are in AGENTS.md, section "Feature: approval limits by role". Read that first.
## Boundary table (amount, role, alone) -> result
50,000.00 Manager -> accept | 50,000.01 Manager -> ExceedsApprovalLimit
500,000.00 Director -> accept | 500,000.01 Director -> first approval only, invoice stays Pending
## Rejection precedence
InvalidState, NotAnApprover, ExceedsApprovalLimit, SameApprover, SecondApproverMustBeDirector.
## Pitfalls: check every diff against these
1. State changed before validation (first approver recorded even though the call was rejected).
2. A single Director completes an invoice above 500,000.
3. Same-user check compares roles instead of ids, or only checks the latest approval, or is case-sensitive.
4. A Manager is accepted as second approver.
5. A rejected second attempt overwrites or clears the first approver.
6. Off-by-one at 500,000 versus 500,000.01, or double instead of decimal.
7. Admin bypasses the limits.
8. Extra rules nobody asked for.