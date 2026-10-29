Prompt:

Plan, but don’t edit: where would you implement a Manager 50k approval limit, how would you test it, and which commands would you run?

BEFORE EXTENDING AGENTS.md

Optimized tool selectionI’ll inspect the approval flow and the existing test structure so I can map the exact implementation and verification points without changing code.

Searched for regex `Manager|50k|50,000|approval limit|PendingApproval|Approver|Approved|Reject` (`**/backend/**`), 52 results

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/src/InvoiceApp.Domain/ApprovalService.cs)

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs)

I’ve confirmed the domain rule lives in the service layer; I’m checking whether the repo already defines manager/role semantics or a requirement document for the 50k threshold before mapping the test plan.

Searched for regex `50k|50,000|Manager|manager|approval limit|role Approver|RequireRole\("` (`**/*.{md,cs,json}`), 4 results

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/ASSIGNMENT.md)

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/src/InvoiceApp.Domain/Invoice.cs)

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/src/InvoiceApp.Api/Auth/UserSession.cs)

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/src/InvoiceApp.Api/Controllers/ApprovalsController.cs)

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/src/InvoiceApp.Api/Program.cs)

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/src/InvoiceApp.Api/Auth/SessionAuthMiddleware.cs)

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/src/InvoiceApp.Api/appsettings.json)

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/src/InvoiceApp.Api/Controllers/AuthController.cs)

## Recommended fit for the rule

This repo already says the business rule belongs in the domain layer: AGENTS.md says business rules go in the Domain project, not controllers or startup code. So I would implement the Manager approval cap in ApprovalService.cs, not in ApprovalsController.cs.

The pattern I’d use is:

- Keep the current `EnsurePending(invoice)` guard
- Add a rule before the invoice is marked `Approved`
- Pass the caller’s role from the session into the service, for example:
  - `Approve(string invoiceId, string approver, string role)`
  - or a small policy method like `EnsureApprovalAllowed(invoice, role)`

Pseudo-logic:

- if `invoice.Amount <= 50000m`, an `Approver` can approve
- if `invoice.Amount > 50000m`, only a `Manager` (or `Admin`, if that is intended to bypass) can approve
- if the role is not allowed, throw `InvalidOperationException` or `UnauthorizedAccessException`

Then the API controller would only pass `HttpContext.GetSession().Role` from UserSession.cs and the current session middleware in SessionAuthMiddleware.cs.

I would also update the role list in appsettings.json and the auth flow in AuthController.cs if `Manager` is a new user role.

---

## Test plan

The right place for tests is ApprovalServiceTests.cs, because the rule is business logic and there are already service-level tests there.

I would add these tests:

1. `Approve_AllowsApprover_WhenAmountIsAtOrBelow50k`
   - invoice amount = 50,000
   - role = `Approver`
   - expect status = `Approved`

2. `Approve_RejectsApprover_WhenAmountExceeds50k`
   - invoice amount = 50,001
   - role = `Approver`
   - expect `InvalidOperationException`

3. `Approve_AllowsManager_WhenAmountExceeds50k`
   - invoice amount = 50,001
   - role = `Manager`
   - expect status = `Approved`

4. `Approve_RejectsManager_WhenAmountExceedsManagerLimit` if your product defines a stricter cap for Manager
   - amount > 50,000 and role = `Manager`
   - expect failure if the limit is “Manager max 50k”

5. `Approve_AllowsAdmin_RegardlessOfAmount` if Admin is the override role

This matches the repo’s current xUnit pattern and keeps the tests focused on real behavior, not mock-only conditions.

---

## Commands I would run

For the fast loop while implementing the rule:

```bash
dotnet test backend/tests/InvoiceApp.Tests/InvoiceApp.Tests.csproj --filter "ApprovalServiceTests"
```

For the full solution check once the rule is in place:

```bash
dotnet test backend/InvoiceApp.sln
```

If I also wanted a manual API smoke test after wiring the role into the controller:

```bash
dotnet run --project backend/src/InvoiceApp.Api
```

Then sign in as a Manager-like demo user and hit the approval endpoint to confirm the behavior end-to-end.

> The key design choice is: policy in the domain, role passed from the API session, and tests in the domain test project.

AFTER EXTENDING AGENTS.md

I’m mapping the existing approval rules and tests in the app so I can give you a concrete implementation and validation plan without changing code.

Searched for regex `ApprovalService|Manager|50,000|500,000|ApprovalPolicy` (`**/backend/**/*.cs`), 9 results

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/src/InvoiceApp.Domain/ApprovalService.cs)

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs)

The current code is still a simple approve/reject flow with no policy enforcement, so I’m checking the domain model and test conventions to anchor the plan to the repo’s intended approval-limit pattern.

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/src/InvoiceApp.Domain/Invoice.cs)

Searched for regex `Approvals|ApprovalPolicy|RequireRole\(|Director|Manager` (`**/backend/**/*.cs`), 17 results

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/src/InvoiceApp.Api/Auth/UserSession.cs)

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/src/InvoiceApp.Api/Auth/RequireRoleAttribute.cs)

Read [](file:///c%3A/Users/balpreetsingh/OneDrive%20-%20Nagarro/NAGP/AI%20Assignment/ai-sdlc-demo-app/backend/src/InvoiceApp.Api/Controllers/ApprovalsController.cs)

## Where I’d implement the Manager 50k rule

Based on the current code, the right place is the domain layer, not the controller:

- ApprovalService.cs is the decision point today; it currently calls `Approve(...)` and immediately flips the invoice to `Approved`.
- ApprovalsController.cs already has the session’s role available via `HttpContext.GetSession().Role`, so the controller can pass both user and role into the domain decision.
- AGENTS.md explicitly says the approval rules belong in Domain and should be a pure `ApprovalPolicy`/policy check before mutation.

So I would add a domain-level policy check before the invoice is mutated, likely:

1. Add named constants in Domain for approval limits, e.g. Manager `50_000m`.
2. Add a pure rule object or helper (per the repo convention, `ApprovalPolicy`) that answers:
   - Can this role approve this amount alone?
   - If not, return a typed rejection reason and do not mutate the invoice.
3. In `ApprovalService.Approve(...)`, call that policy first and only update `Status`, `DecidedBy`, `DecidedAt` if the decision is accepted.

This matches the repo’s guidance that the “rule logic stays in Domain; controllers only translate HTTP.”

---

## How I’d test it

The repo already favors xUnit tests in ApprovalServiceTests.cs, and the AGENTS note says to cover:

- boundary cases: `50,000`, `50,000.01`, `500,000`, `500,000.01`
- an “invoice unchanged” assertion after each rejection

I’d add tests like:

- `Manager_CanApprove_At_50000_Alone`
- `Manager_CannotApprove_Above_50000_Alone`
- `Manager_Approval_At_50000_01_IsRejected_And_Invoice_Unchanged`
- `Director_CanApprove_At_500000_Alone`
- `Above_500000_Requires_Two_Approvals_And_Second_Must_Be_Director`

For the specific Manager limit, the key assertion pattern is:

- if amount is `<= 50,000` and the user role is Manager, approval succeeds
- if amount is `> 50,000`, approval is rejected without mutating status, decision timestamp, approver, or approvals list

The repo currently has no approval policy tests at all, so this is a natural extension of the existing test file.

---

## Commands I’d run

For the unit-level approval logic:

- `dotnet test InvoiceApp.sln --filter "ApprovalService"`
- `dotnet test backend/InvoiceApp.sln`

If the feature also needs browser validation for the role-based UI flow:

- `cd e2e && npm install && npx playwright install chromium && npm test`

I would start with the targeted `ApprovalService` filter, then run the full solution test suite before calling the change ready.