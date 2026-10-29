# Project conventions for AI agents

Invoice approval demo app: ASP.NET Core 8 Web API with a small static web UI.

## Layout

- `backend/src/InvoiceApp.Domain`: business rules, no web or storage code. `ApprovalService` owns the invoice lifecycle (Draft, PendingApproval, Approved, Rejected).
- `backend/src/InvoiceApp.Api`: controllers, sign-in (`Auth/`), in-memory repository, and the web UI under `wwwroot/`.
- `backend/tests/InvoiceApp.Tests`: xUnit tests, one test class per class under test, named `<ClassUnderTest>Tests`.
- `e2e`: Playwright end-to-end tests (`tests/`) and page objects (`pages/`).
- `docs/use-cases`: short briefs for features we may build.

## Conventions

- Controllers live in `Controllers/`, one per resource, route `api/[controller]`. Request bodies are `record` types declared inside the controller.
- Controllers inherit `ApiControllerBase` and use `Handle(...)` so service errors map to 404, 409 and 400.
- Every endpoint is behind sign-in. Restrict actions with `[RequireRole("...")]`: destructive actions are Admin only, approvals are Approver or Admin.
- Business rules go in the Domain project, never in controllers.
- Register services in `Program.cs`. Repositories are interfaces in Domain with an in-memory implementation in Api.
- Every new rule or endpoint needs tests. Use `FakeInvoiceRepository` in tests.
- Use clear names; no one-letter variables or `flag` parameters.
- End-to-end tests use page objects and role-based locators (`getByRole`, `getByLabel`), never CSS or XPath selectors.

## Commands

- Run the app: `dotnet run --project backend/src/InvoiceApp.Api` then open http://localhost:5000
- Run the tests: `dotnet test backend/InvoiceApp.sln`
- Run end-to-end tests: `cd e2e && npm install && npx playwright install chromium && npm test`

## Demo accounts (not real credentials)

- `demo.approver` / `ChangeMe123!` (role Approver)
- `demo.admin` / `ChangeMe123!` (role Admin)

## Never

- Commit real credentials, tokens or customer data.
- Add business logic to `Program.cs` or controllers.

## Approval limits (role-approval-limits)
- Amounts are `decimal`; limits are inclusive and live as named constants in one place in Domain.
- Manager: may approve alone up to 50,000. Director: alone up to 500,000.
- Above 500,000: two approvals from two different users; the second must be a Director.
  The first must be a Manager or Director (assumption D2).
- The same user id can never count twice on one invoice (case-insensitive compare).
- A rejected attempt returns a typed reason (code + message) and MUST NOT mutate the
  invoice: status, approvals list, timestamps all unchanged.
- Out of scope: do not invent extra rules (e.g. creator-cannot-approve).

## Layering for this feature
- Pure rule logic in Domain (`ApprovalPolicy`, no side effects); `ApprovalService` calls it
  first and mutates only on acceptance. Controllers only map results to HTTP.

## Test and review expectations
- Tests derive from the OpenSpec scenarios, not from reading the implementation.
- Every rule needs boundary tests (50,000 / 50,000.01 / 500,000 / 500,000.01) and an
  "invoice unchanged" assertion after every rejection.
- Commands: tests `dotnet test backend/InvoiceApp.sln`;
  mutation `cd backend/tests/InvoiceApp.Tests && dotnet stryker`; e2e as above.
- Reviews are written as JSON per `docs/ai/review-schema.json`; every finding gets a verdict
  of fixed, dismissed (reason) or deferred (reason).