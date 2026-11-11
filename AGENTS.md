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

## Feature: approval limits by role (OpenSpec change: add-role-approval-limits)
### Rules (amounts are decimal; limits are inclusive)
- Manager may approve alone when Amount <= 50,000.
- Director may approve alone when Amount <= 500,000.
- Amount > 500,000 needs two approvals from two different users. The first approver must be a
Manager or Director. The second approver must be a Director.
- After the first approval of an invoice above 500,000 the invoice stays Pending with one entry in
its approvals list. Only the second valid approval makes it Approved.
- The same user id never counts twice on one invoice. Compare ids case-insensitively.
- Admin and every other role cannot approve. The seeded user demo.approver becomes a Manager.
New seeded users: demo.director and demo.director2. <adjust to recon if needed>
- A rejected attempt returns a reason code and a human-readable message and changes NOTHING on the
invoice: not the status, not the approvals list, not any timestamp.
- Reason codes: NotAnApprover, ExceedsApprovalLimit, RequiresTwoApprovers (interim only),
SameApprover, SecondApproverMustBeDirector, InvalidState (map to existing not-pending behaviour).
- If several rejections apply, report the first of: InvalidState, NotAnApprover,
ExceedsApprovalLimit, SameApprover, SecondApproverMustBeDirector.
- Out of scope, do not add: creator-cannot-approve, currency, delegation, audit log, persistence.
### Layering for this feature
- Domain: a pure ApprovalPolicy (no I/O, no mutation) and an ApprovalService that calls the policy
first and mutates the invoice only after the policy accepted. Limits are named constants in one
place only.
- API: controllers only map the result to HTTP using the existing error handling. No business rules
in controllers.
- UI: show the rejection message in an element with role="alert".
### Tests
- Unit test command: <from recon, e.g. dotnet test backend/InvoiceApp.sln>
- E2E command: cd e2e && npm install && npx playwright install chromium && npm test
- Tests are written from the spec scenarios, not from reading the implementation.
- Every rule needs boundary tests at 50,000 / 50,000.01 / 500,000 / 500,000.01.
- After every rejection a test asserts the invoice equals its snapshot from before the call.
- Assert reason codes, not message text.
### Review expectations
- Every review finding has: severity, file, line, finding, suggested fix.
- Every finding on the pull request gets one verdict: fixed (with commit), dismissed (with reason),
or deferred (with reason and issue link).
- Never commit code you have not read. Never claim tests pass without running them.
### Git conventions
- Branch: feature/role-approval-limits. One logical change per commit. The message says what
changed and why.