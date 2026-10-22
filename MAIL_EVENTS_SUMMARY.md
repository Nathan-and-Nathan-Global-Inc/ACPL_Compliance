# Mail Events Summary

## Scope

The application defines **15 mail events**. Each event inserts a `Class_tb_mailer` record into the shared mail queue; this repository does not send SMTP mail directly.

Recipient email addresses are read from `acpl_common.dbo.employees.empl_mail` using the IDs listed below.

## Recipient ID Definitions

- **Plan creator ID**: `test_plan_hdr.plan_by_slno`
- **Plan verifier IDs**: `test_plan_verifiers.ver_empl_slno` for the current `tp_hdr_slno`
- **Validation creator ID**: `validation_hdr.created_by_slno`
- **Conditional verifier ID**: `validation_hdr.verified_by`
- **Final-release verifier ID**: `validation_hdr.finalrel_verified_by`
- **QA head IDs**: all `employees.empl_slno` values where `dept_slno IN (39, 40)`, `department_head = 1`, and `del_status = 0`

## Test Plan Mail Events

| # | Event / Subject | Screen Name | To ID | CC ID |
|---|---|---|---|---|
| TP-1 | Test Plan Submitted | Test Plan | All plan verifier IDs (`test_plan_verifiers.ver_empl_slno`) | Plan creator ID (`plan_by_slno`) |
| TP-2 | Test Plan Verified | Test Plan | All active QA head IDs (departments 39/40) | Plan creator ID + all plan verifier IDs |
| TP-3 | Test Plan Verification Rejected | Test Plan | Plan creator ID | All plan verifier IDs |
| TP-4 | Test Plan Approved | Test Plan | Plan creator ID + all plan verifier IDs | All active QA head IDs (departments 39/40) |
| TP-5 | Test Plan Approval Rejected | Test Plan | Plan creator ID + all plan verifier IDs | All active QA head IDs (departments 39/40) |

## Conditional Release Mail Events

| # | Event / Subject | Screen Name | To ID | CC ID |
|---|---|---|---|---|
| CR-1 | Conditional Release - Best Trial Submitted | Test Plan Qualification | Conditional verifier ID (`verified_by`) | Validation creator ID (`created_by_slno`) |
| CR-2 | Conditional Release - Trial Details Verified | Conditional Release Verification | All active QA head IDs (departments 39/40) | Validation creator ID + conditional verifier ID |
| CR-3 | Conditional Release - Trial Details Verification Rejected | Conditional Release Verification | Validation creator ID | Conditional verifier ID |
| CR-4 | Conditional Release - Trial Details Approved | Conditional Release Approval | Validation creator ID + conditional verifier ID | All active QA head IDs (departments 39/40) |
| CR-5 | Conditional Release - Trial Details Approval Rejected | Conditional Release Approval | Validation creator ID + conditional verifier ID | All active QA head IDs (departments 39/40) |

## Final Release Mail Events

| # | Event / Subject | Screen Name | To ID | CC ID |
|---|---|---|---|---|
| FR-1 | Final Release - Best Trial Submitted | Test Plan Qualification | Final-release verifier ID (`finalrel_verified_by`) | Validation creator ID (`created_by_slno`) |
| FR-2 | Final Release - Trial Details Verified | Final Release Verification | All active QA head IDs (departments 39/40) | Validation creator ID + final-release verifier ID |
| FR-3 | Final Release - Trial Details Verification Rejected | Final Release Verification | Validation creator ID | Final-release verifier ID |
| FR-4 | Final Release - Trial Details Approved | Final Release Approval | Validation creator ID + final-release verifier ID | All active QA head IDs (departments 39/40) |
| FR-5 | Final Release - Trial Details Approval Rejected | Final Release Approval | Validation creator ID + final-release verifier ID | All active QA head IDs (departments 39/40) |

## Event Flow Summary

1. A submission mail goes to the assigned verifier, with the creator copied.
2. After verification, a mail goes to the QA department heads, with the creator and verifier(s) copied.
3. A verification rejection goes to the creator, with the verifier(s) copied.
4. After approval, the creator and verifier(s) are notified, with the QA department heads copied.
5. An approval rejection uses the same recipient pattern as approval completion.

## Implementation References

- Test Plan mail methods: `Controllers/TestPlanController.cs`, lines 1818-2398
- Validation mail methods: `Controllers/ValidationController.cs`, lines 2425-3606
- Mail queue fields: `screen_name`, `send_reason`, `subject`, `send_to`, and `cc_to`

## Notes

- Recipient IDs are dynamic database values, so this source code does not contain fixed employee numbers or email addresses.
- Multiple recipient email addresses are combined into comma-separated strings.
- Null or blank employee email addresses may result in an empty or partial `send_to`/`cc_to` value because mail records are queued without recipient validation.
- Conditional and final submission mails are generated only for Machine or Material qualifications with a matching release type.
