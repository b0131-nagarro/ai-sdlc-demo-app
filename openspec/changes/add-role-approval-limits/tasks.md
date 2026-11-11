## 1.x Phase 1: single-approver limits
- [ ] Roles and seed users: demo.approver becomes Manager; add Director users; confirm Admin cannot approve.
- [ ] ApprovalPolicy implements the single-approver limits by role and the rejection reasons.
- [ ] ApprovalService orchestrates validation and state mutation after the policy accepts.
- [ ] API error mapping converts approval rejections into existing HTTP/error behavior.
- [ ] Unit tests T1 to T9 cover the single-approver scenarios and unchanged-invoice assertions.
- [ ] UI message for rejections is displayed in an alert element.
- [ ] Interim rule T6: Director approves 500,000.01 and is rejected with RequiresTwoApprovers.

## 2.x Phase 2: two-approver rule
- [ ] Approvals list on the invoice records each approver by user id.
- [ ] Second-approver logic enforces the Director-only second approval and does not allow duplicate users.
- [ ] Replace the interim rule and its test T6 with the final above-500,000 behavior.
- [ ] UI message when the invoice stays Pending after a first approval is surfaced to the user.
- [ ] Unit tests U1 to U12 cover the two-approver flow, precedence, unchanged-state checks, and boundary behavior.
