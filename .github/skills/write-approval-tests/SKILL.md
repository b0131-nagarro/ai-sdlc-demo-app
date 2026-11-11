---
name: write-approval-tests
description: How to write unit tests for invoice approval rules with boundaries and unchanged-state checks.
---
# Writing approval tests
- Derive tests from the scenarios in openspec/changes/add-role-approval-limits/specs/, not from the code.
- Use the existing test framework and the existing fake repository; copy the style of neighbouring tests.
- Name each test with its scenario id from the spec, for example T2_Manager_Above50k_IsRejected.
- Boundaries 50,000 / 50,000.01 / 500,000 / 500,000.01 are mandatory.
- After EVERY rejection assert the whole invoice equals a snapshot taken before the call
(status, approvals list, every timestamp).
- Assert the reason CODE, never the message text.
- One behaviour per test. Use parameterised tests for boundary tables.
- Forbidden: tests that only assert "no exception", mocks of the unit under test, shared mutable state
between tests, skipped or ignored tests.