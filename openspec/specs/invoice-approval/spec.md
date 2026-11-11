# Invoice approval role limits

## Requirements

### Requirement: THE SYSTEM SHALL enforce role-based single-approver limits
The system SHALL enforce approval limits by role so that a Manager can approve alone only when the invoice total is less than or equal to 50,000, and a Director can approve alone only when the invoice total is less than or equal to 500,000.

#### Scenario: Manager approves at the Manager limit
- Given an invoice amount of 50,000.00
- And the current user has role Manager
- When the user approves the invoice
- Then the approval succeeds
- And the invoice status becomes Approved

#### Scenario: Manager exceeds the Manager limit
- Given an invoice amount of 50,000.01
- And the current user has role Manager
- When the user approves the invoice
- Then the system rejects the approval with reason ExceedsApprovalLimit
- And the invoice remains unchanged

#### Scenario: Director approves at the Director limit
- Given an invoice amount of 500,000.00
- And the current user has role Director
- When the user approves the invoice
- Then the approval succeeds
- And the invoice status becomes Approved

#### Scenario: Admin is not an approver
- Given an invoice amount of 600,000.00
- And the current user has role Admin
- When the user approves the invoice
- Then the system rejects the approval with reason NotAnApprover
- And the invoice remains unchanged

### Requirement: THE SYSTEM SHALL reject invalid states and duplicate approvals
The system SHALL reject approval attempts when the invoice is not pending, when the user is not eligible to approve, and when the same user attempts to approve more than once for the same invoice, with the invoice left unchanged in every rejection case.

#### Scenario: Approval against a non-pending invoice is rejected
- Given an invoice with status Draft
- And the current user has role Manager
- When the user approves the invoice
- Then the system rejects the approval with reason InvalidState
- And the invoice remains unchanged

#### Scenario: Duplicate approvals are rejected case-insensitively
- Given an invoice with a prior approval from user id "alice"
- And the current user id is "ALICE"
- When the user approves the invoice again
- Then the system rejects the approval with reason SameApprover
- And the invoice remains unchanged

#### Scenario: A non-approver role is rejected
- Given an invoice amount of 600,000.00
- And the current user has no approver role
- When the user approves the invoice
- Then the system rejects the approval with reason NotAnApprover
- And the invoice remains unchanged

### Requirement: THE SYSTEM SHALL require a Director second approver on high-value invoices
The system SHALL require two distinct approvers for invoice amounts strictly greater than 500,000, where the first approver may be a Manager or Director and the second approver must be a Director.

#### Scenario: First approval above the threshold stays pending
- Given an invoice amount of 600,000.00
- And the current user has role Director
- And no prior approval exists on the invoice
- When the user approves the invoice
- Then the approval succeeds
- And the invoice status remains PendingApproval
- And the approvals list contains exactly that user

#### Scenario: Second Director approval completes the invoice
- Given an invoice amount of 600,000.00
- And user A has already approved the invoice
- And the current user has role Director and user id different from A
- When the user approves the invoice
- Then the approval succeeds
- And the invoice status becomes Approved
- And the approvals list contains A and the current user

#### Scenario: Manager cannot be the second approver on a large invoice
- Given an invoice amount of 600,000.00
- And user A has already approved the invoice
- And the current user has role Manager
- When the user approves the invoice
- Then the system rejects the approval with reason SecondApproverMustBeDirector
- And the invoice remains unchanged

#### Scenario: Manager may be the first approver on a large invoice
- Given an invoice amount of 600,000.00
- And the current user has role Manager
- And no prior approval exists on the invoice
- When the user approves the invoice
- Then the approval succeeds
- And the invoice status remains PendingApproval
- And the approvals list contains exactly that manager

### Requirement: THE SYSTEM SHALL apply rejection precedence in the correct order
The system SHALL evaluate approval rejections in the order InvalidState, NotAnApprover, ExceedsApprovalLimit, SameApprover, and SecondApproverMustBeDirector and return the first applicable reason.

#### Scenario: Invalid state takes precedence over role checks
- Given an invoice with status Draft
- And the current user has role Admin
- When the user approves the invoice
- Then the system rejects the approval with reason InvalidState
- And the invoice remains unchanged

#### Scenario: Same approver takes precedence over second-approver validation
- Given an invoice amount of 600,000.00
- And the first approver is a Manager
- And the current user is that same manager
- When the user approves the invoice again
- Then the system rejects the approval with reason SameApprover
- And the invoice remains unchanged
