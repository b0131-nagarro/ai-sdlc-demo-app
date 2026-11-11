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
- Every endpoint is behind sign-in. Restrict actions with `[RequireRole("...")]`: destructive actions are Admin only; approvals are Manager or Director (Admin cannot approve, see the Feature section).
- Business rules go in the Domain project, never in controllers.
- Register services in `Program.cs`. Repositories are interfaces in Domain with an in-memory implementation in Api.
- Every new rule or endpoint needs tests. Use `FakeInvoiceRepository` in tests.
- Use clear names; no one-letter variables or `flag` parameters.
- End-to-end tests use page objects and role-based locators (`getByRole`, `getByLabel`), never CSS or XPath selectors.

## Commands

- Run the app: `dotnet run --project backend/src/InvoiceApp.Api` then open http://localhost:5000
- Run the tests: `dotnet test backend/InvoiceApp.sln`
- Run end-to-end tests: `cd e2e && npm install && npx playwright install chromium && npm test`

## Demo accounts (not real credentials; the Director users are created by the approval-limits feature)

- `demo.approver` / `ChangeMe123!` (role Manager; it was Approver before the approval-limits feature)
- `demo.director` / `ChangeMe123!` (role Director)
- `demo.director2` / `ChangeMe123!` (role Director)
- `demo.admin` / `ChangeMe123!` (role Admin; cannot approve)

## Never

- Commit real credentials, tokens or customer data.
- Add business logic to `Program.cs` or controllers.

## Feature: approval limits by role (OpenSpec change: add-role-approval-limits)

### Rules (amounts are decimal; limits are inclusive)
- Manager may approve alone when Amount <= 50,000.
- Director may approve alone when Amount <= 500,000.
- Amount > 500,000 needs two approvals from two different users. The first approver must be a Manager or Director. The second approver must be a Director.
- After the first approval of an invoice above 500,000 the invoice stays PendingApproval with one entry in its approvals list. Only the second valid approval makes it Approved.
- The same user id never counts twice on one invoice. Compare ids case-insensitively.
- Admin and every other role cannot approve. The seeded user demo.approver becomes a Manager. New seeded users: demo.director and demo.director2.
- Invoice statuses already exist: Draft, PendingApproval, Approved, Rejected. Do not add a status.
- A rejected attempt returns a reason code and a human-readable message and changes NOTHING on the invoice: not the status, not the approvals list, not any timestamp.
- Reason codes: NotAnApprover, ExceedsApprovalLimit, RequiresTwoApprovers (interim only), SameApprover, SecondApproverMustBeDirector, InvalidState (map to existing not-pending behaviour).
- If several rejections apply, report the first of: InvalidState, NotAnApprover, ExceedsApprovalLimit, SameApprover, SecondApproverMustBeDirector.
- Out of scope, do not add: creator-cannot-approve, currency, delegation, audit log, persistence.

### Decisions (the spec, tasks and prompts refer to these by id)
- D1 Roles: add Manager and Director. Admin and every other role cannot approve (NotAnApprover). demo.approver becomes a Manager. New seeded users: demo.director and demo.director2. NotAnApprover may come from the existing authorization layer instead of the domain.
- D2 First approver above 500,000: Manager or Director. Only the second must be a Director.
- D3 Amounts are decimal, limits are inclusive, no currency handling.
- D4 Reason precedence: InvalidState, NotAnApprover, ExceedsApprovalLimit, SameApprover, SecondApproverMustBeDirector. Report the first one that applies.
- D5 First approval above 500,000: accepted; the invoice stays PendingApproval with one approvals entry.
- D6 Interim rule (implementation step 1 only): invoices above 500,000 are rejected with RequiresTwoApprovers until the two-approver step replaces it.
- D7 Scope: no other rules, no audit log, no persistence changes, no creator-cannot-approve.
- D8 Limit semantics: a role limit applies when that user would approve ALONE (amount <= 500,000). ExceedsApprovalLimit exists only on that single-approver path. Above 500,000 the two-approver rule replaces the amount limit: a Manager may be the first approver (D2) and is never the second.
- D9 Amounts are compared exactly as stored: no rounding or truncation before comparing. Only an amount strictly greater than 500,000 needs two approvers (500,000.01 does; 500,000.00 does not).
- D10 User ids come from the authenticated user. Compare them case-insensitively (ordinal); do not trim, Unicode-fold or otherwise normalise them.

### Layering for this feature
- Domain: a pure ApprovalPolicy (no I/O, no mutation) and an ApprovalService that calls the policy first and mutates the invoice only after the policy accepted. Limits are named constants in one place only.
- API: controllers only map the result to HTTP using the existing error handling. No business rules in controllers.
- UI: show the rejection message in an element with role="alert".

### Tests
- Unit test command: dotnet test backend/InvoiceApp.sln
- E2E command: cd e2e && npm install && npx playwright install chromium && npm test
- Tests are written from the spec scenarios, not from reading the implementation.
- Every rule needs boundary tests at 50,000 / 50,000.01 / 500,000 / 500,000.01.
- After every rejection a test asserts the invoice equals its snapshot from before the call.
- Assert reason codes, not message text.
- New unit test classes follow the existing convention: ApprovalPolicyTests, ApprovalServiceTests.
- Existing unit and e2e tests that approve as Admin or as the old Approver role are updated to the new roles, and nothing else in them changes.

### Review expectations
- Every review finding has: severity, file, line, finding, suggested fix.
- Every finding on the pull request gets one verdict: fixed (with commit), dismissed (with reason), or deferred (with reason and issue link).
- Never commit code you have not read. Never claim tests pass without running them.

### Git conventions
- Branch: feature/role-approval-limits. One logical change per commit. The message says what changed and why.

## Agents, skills and output format
- Agents (.github/agents): feature-lead (delegates to subagents), spec-writer, implementer, test-writer, reviewer.
- Skills (.github/skills): approval-rules, write-approval-tests, pr-review, run-mutation-testing.
- Logging: hooks in .github/hooks append every prompt and tool call to docs/ai-log/ai-interactions.jsonl.
- Agents never commit. A human reads every diff and commits.
- Reviews go to docs/reviews/review-<n>.json in the schema defined in the pr-review skill; a hook validates the format after every tool call.

### Output format (all agents)
End every reply with exactly one line:
RESULT: <done|blocked> | FILES: <comma-separated paths, or none> | NEXT: <one sentence>