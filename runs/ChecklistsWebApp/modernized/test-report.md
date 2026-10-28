# Test report: ChecklistsWebApp to .NET 10

Scope: whole application
Classification: finished modernization (real persistence, not a prototype)
Date: 2026-10-27

## Result

| Check | Result |
|---|---|
| `dotnet restore` | Passed |
| `dotnet build` | Passed, 0 warnings, 0 errors |
| `dotnet test` | Passed, 152 of 152 (151 scenario tests plus 1 coverage test) |
| `compare-parity.ps1 -WholeApp` with real login | `=== PARITY PASSED ===` |
| Backlog scenarios with no passing test | None |

## Commands and output

Run from `runs/ChecklistsWebApp/modernized/`.

```text
> dotnet restore ChecklistsModern.slnx
  Restored ...\ChecklistsModern.Tests\ChecklistsModern.Tests.csproj (in 369 ms).
  1 of 2 projects are up-to-date for restore.

> dotnet build ChecklistsModern.slnx
Build succeeded.
    0 Warning(s)
    0 Error(s)

> dotnet test ChecklistsModern.slnx --no-build --logger "trx;LogFileName=results.trx" --results-directory TestResults
Passed!  - Failed:     0, Passed:   152, Skipped:     0, Total:   152, Duration: 46 s - ChecklistsModern.Tests.dll (net10.0)
```

Parity, run from the repo root with the app started in Development on `http://localhost:5181` (fresh seeded SQLite database):

```text
> agent-config\scripts\compare-parity.ps1 -LegacyRoot runs\ChecklistsWebApp\legacy\Checklists
    -ModernRoot runs\ChecklistsWebApp\modernized\ChecklistsModern -ParityMap runs\ChecklistsWebApp\modernized\parity-map.md
    -BaseUrl http://localhost:5181 -WholeApp -LoginUrl /Account/Login -LoginFields "Email=author@checklists.local;Password=<preview password>"
Controller Controllers\AccountController.cs: 14 old actions, 14 mapped
Controller Controllers\Apis\ChecklistsController.cs: 1 old action, 1 mapped
Controller Controllers\Apis\TasksController.cs: 3 old actions, 3 mapped
Controller Controllers\ChecklistsController.cs: 4 old actions, 4 mapped
Controller Controllers\ManageController.cs: 12 old actions, 12 mapped
Controller Controllers\TasksController.cs: 4 old actions, 4 mapped
Whole app: 6 legacy entry points checked
Signed in through http://localhost:5181/Account/Login (landing page http://localhost:5181/Home returned: (404) Not Found.)
Found ... (96 label and field checks, all found)
Stylesheet loaded http://localhost:5181/Content/bootstrap.css (127317 bytes)
Stylesheet loaded http://localhost:5181/Content/bootstrap-paper.css (179125 bytes)
Stylesheet loaded http://localhost:5181/Content/Site.css (2660 bytes)
=== PARITY PASSED ===
```

The full parity output is in `parity.log`. The landing page 404 is the legacy behavior in US-AC-01: a login with no return URL goes to `/Home`, which does not exist in legacy either. The same run with a wrong password still ends with `=== PARITY FAILED ===` and `Login with -LoginFields did not succeed`.

## Fix loop (retry budget 3)

| Attempt | Result | Cause | Fix |
|---|---|---|---|
| Tests 1 | 150 of 152 | ASP.NET Core adds `maxlength` to inputs that have `[StringLength]`. MVC 5 did not. With it, the browser stops typing at 255 characters, so the legacy "maximum length" message can no longer appear on the checklist Name and task Title screens. Password boxes got `maxlength="100"` the same way. | Phase 5 fix: `Filters/LegacyHtmlGenerator.cs` removes the generated `maxlength` from text and password boxes, registered in `Program.cs`. |
| Tests 2 | 152 of 152 | | |
| Parity 1 | Failed: `Login POST failed ... (404) Not Found` | Script bug: it followed the post-login redirect and treated the landing page error as a failed login. Every signed-in page was still found. | Playbook fix in `agent-config/scripts/compare-parity.ps1`: a POST that redirects away from the login form counts as signed in. A rejected login (form shown again) still fails. |
| Parity 2 | Passed | | |

## What the tests cover

Every test starts its own copy of the app with `WebApplicationFactory<Program>` and its own SQLite database file, seeded with the preview rows from `plan.md`. Requests go through the real pipeline: routing, cookies, antiforgery, Identity and EF Core.

- **API:** every API scenario (US-CL-04, US-TK-05, US-TK-06, US-TK-07, US-XD-03, US-XD-04) sends the HTTP request and checks the status code, the body (empty, or `{"Message":"..."}`) and the stored row.
- **Validation:** every validation scenario posts the invalid form and checks the legacy message text. Three scenarios post without the antiforgery token and expect 400 (US-AC-01, US-CL-02, US-TK-02).
- **Authorization:**
  - Anonymous visitors are sent to login (US-CL-01, US-TK-01, US-MG-01, US-AC-03).
  - Another user is sent to login (US-CL-03, US-TK-01, US-TK-02, US-TK-03).
  - Anonymous and other-user API calls return 401 (US-CL-04, US-TK-05, US-TK-06, US-TK-07, US-XD-04).
  - The approved legacy gaps are tested as legacy behavior: US-CL-03 (rename through Save with no owner check) and US-TK-04 (task Save with no owner check, including anonymous).
- **Real persistence:** EF Core on a SQLite file, not an in-memory fake. Saves are read back in a later request or from a new `DbContext` (for example US-CL-02, US-TK-02, US-MG-02, US-MG-05, US-AC-02, US-AC-05).
- **Cross-domain:** each of the 5 dependencies in `knowledge-graph.md` has a test that goes through both domains (US-XD-01 to US-XD-05).
- **Scenario coverage:** `ScenarioCoverageTests` reads `backlog.md` and fails if any `Scenario:` title has no test with that title in its name.

## Not tested

- The SQL Server provider. It is configured (`Database:Provider = SqlServer`) but no SQL Server was available. Only SQLite is tested.
- Real email and SMS delivery. As in legacy, the senders do nothing (`NoOpEmailSender`, `NoOpSmsSender`).
- External login providers (Google, Facebook and others). None are configured, matching legacy. Only the "not configured" paths are tested.
- Client-side JavaScript (bootbox confirm, the checkbox toggle). The tests check that the legacy scripts and URLs are on the page, and the API calls those scripts make are tested directly. No browser runs the scripts.

## Changes found while porting and testing

- Backlog corrections from Phase 5, recorded in `backlog.md`:
  - US-CL-02 and US-CL-03: Save redirects to `/`, not `/Checklists`.
  - US-MG-04: removing a login the user does not have redirects with `Message=RemoveLoginSuccess`.
- `maxlength` removal: the Phase 6 fix above, so the screens behave like legacy.
- `System.Security.Cryptography.Pkcs` is pinned to 10.0.14. Without the pin, the transitive 9.0.11 version raises the NU1902 and NU1903 vulnerability warnings.

## Scenario to test

Every row passed. Test names are `<story> | <Scenario title>`.

| Story | Scenario / test name | Outcome |
|---|---|---|
| (coverage) | Every Scenario title in backlog.md has a test with that title | Passed |
| US-AC-01 | A correct login with a local return URL goes back to that URL | Passed |
| US-AC-01 | A correct login with no return URL goes to /Home | Passed |
| US-AC-01 | A locked-out user sees the lockout page | Passed |
| US-AC-01 | A login with a non-local return URL goes to /Home | Passed |
| US-AC-01 | A user with two-factor enabled is sent to choose a code provider | Passed |
| US-AC-01 | A wrong password shows Invalid login attempt | Passed |
| US-AC-01 | An email that is not an email address shows the format message | Passed |
| US-AC-01 | An empty email and password show the required messages | Passed |
| US-AC-01 | Login without the verification token is rejected | Passed |
| US-AC-01 | The login page shows the email, password, and remember me fields | Passed |
| US-AC-01 | Wrong passwords never lock the account | Passed |
| US-AC-02 | A full name longer than 255 characters shows the length message | Passed |
| US-AC-02 | A missing full name shows the required message | Passed |
| US-AC-02 | A password shorter than 6 characters shows the length message | Passed |
| US-AC-02 | A password that breaks the rules shows the password message | Passed |
| US-AC-02 | A registration confirmation that does not match shows the compare message | Passed |
| US-AC-02 | A valid registration creates the user, signs them in, and opens the list | Passed |
| US-AC-02 | Registering an email that already exists shows the duplicate messages | Passed |
| US-AC-02 | The register page shows the account fields | Passed |
| US-AC-03 | Log out is POST only | Passed |
| US-AC-03 | Log out requires a signed-in user | Passed |
| US-AC-03 | Log out signs me out and returns to the list URL | Passed |
| US-AC-04 | A confirmed email shows the forgot password form again | Passed |
| US-AC-04 | An email that is not confirmed shows the confirmation page | Passed |
| US-AC-04 | An invalid email shows the format message | Passed |
| US-AC-04 | An unknown email shows the confirmation page | Passed |
| US-AC-04 | The forgot password page shows the email field | Passed |
| US-AC-05 | A reset for an unknown email goes to the confirmation page | Passed |
| US-AC-05 | A reset with a bad code shows Invalid token | Passed |
| US-AC-05 | A reset with a valid code changes the password | Passed |
| US-AC-05 | The reset confirmation page links to the login page | Passed |
| US-AC-05 | The reset page with a code shows the reset form | Passed |
| US-AC-05 | The reset page without a code shows the error page | Passed |
| US-AC-06 | Confirm email with a bad code shows the error page | Passed |
| US-AC-06 | Confirm email with a missing user id or code shows the error page | Passed |
| US-AC-06 | Confirm email with a valid code confirms the email | Passed |
| US-AC-07 | A correct code signs the user in | Passed |
| US-AC-07 | A wrong code shows Invalid code | Passed |
| US-AC-07 | After the password step the send code page lists the providers | Passed |
| US-AC-07 | Choosing a provider goes to the verify code page | Passed |
| US-AC-07 | Send code without a password step shows the error page | Passed |
| US-AC-07 | Verify code without a password step shows the error page | Passed |
| US-AC-08 | External login confirmation when already signed in goes to Manage | Passed |
| US-AC-08 | External login confirmation with no external sign-in shows the failure page | Passed |
| US-AC-08 | External login with a provider that is not configured does not sign anyone in | Passed |
| US-AC-08 | The external login callback with no external sign-in goes back to login | Passed |
| US-AC-08 | The external login failure page shows its message | Passed |
| US-CL-01 | A soft-deleted checklist is hidden from the list | Passed |
| US-CL-01 | A user with no checklists sees the empty message | Passed |
| US-CL-01 | An anonymous visit to the checklist list is sent to login | Passed |
| US-CL-01 | Each checklist has edit and delete controls | Passed |
| US-CL-01 | Signed-in user sees only their own checklists | Passed |
| US-CL-02 | New checklist form shows an empty name | Passed |
| US-CL-02 | Save a new checklist and the next request still shows it | Passed |
| US-CL-02 | Save with a name longer than 255 characters shows the length message | Passed |
| US-CL-02 | Save with an empty name shows the required message | Passed |
| US-CL-02 | Save without the verification token is rejected | Passed |
| US-CL-03 | A different signed-in user can change the name through Save | Passed |
| US-CL-03 | A different signed-in user cannot open the rename form | Passed |
| US-CL-03 | Edit of an unknown id is not found | Passed |
| US-CL-03 | Edit without an id is not found | Passed |
| US-CL-03 | Save of a soft-deleted checklist changes the name and leaves it deleted | Passed |
| US-CL-03 | Save of an unknown id is not found and does not create a checklist | Passed |
| US-CL-03 | The author opens the rename form | Passed |
| US-CL-03 | The author saves a new name | Passed |
| US-CL-04 | A different signed-in user cannot delete the checklist | Passed |
| US-CL-04 | An anonymous delete is rejected | Passed |
| US-CL-04 | Delete of a missing checklist is not found | Passed |
| US-CL-04 | Delete of an already deleted checklist is not found | Passed |
| US-CL-04 | The author deletes a checklist and it leaves the list | Passed |
| US-CL-05 | The list page carries the legacy delete script | Passed |
| US-MG-01 | A status message is shown for each message id | Passed |
| US-MG-01 | The settings page requires a signed-in user | Passed |
| US-MG-01 | The settings page shows password, external logins, and two-factor sections | Passed |
| US-MG-02 | A change password confirmation that does not match shows the compare message | Passed |
| US-MG-02 | A correct change saves the new password and returns to the list | Passed |
| US-MG-02 | A new password shorter than 6 characters shows the length message | Passed |
| US-MG-02 | A wrong current password shows Incorrect password | Passed |
| US-MG-02 | The change password page shows the three password fields | Passed |
| US-MG-03 | A set password confirmation that does not match shows the compare message | Passed |
| US-MG-03 | A user who already has a password cannot set another one | Passed |
| US-MG-03 | The set password page shows the local login form | Passed |
| US-MG-04 | Linking a provider that is not configured does not add a login | Passed |
| US-MG-04 | Removing a login the user does not have goes back to the logins page | Passed |
| US-MG-04 | The error message id shows the error text | Passed |
| US-MG-04 | The link login callback with no external sign-in goes back with the error message | Passed |
| US-MG-04 | With no provider configured the page says so | Passed |
| US-MG-05 | A correct code saves the number | Passed |
| US-MG-05 | A valid number goes to the verify page without sending a text | Passed |
| US-MG-05 | A wrong code shows Failed to verify phone | Passed |
| US-MG-05 | An invalid number shows the phone message | Passed |
| US-MG-05 | Remove phone number clears the number | Passed |
| US-MG-05 | The add phone page shows the number field | Passed |
| US-MG-05 | The verify page with a number shows the code form | Passed |
| US-MG-05 | The verify page without a number shows the error page | Passed |
| US-MG-06 | Disable two-factor turns it off and returns to Manage | Passed |
| US-MG-06 | Enable two-factor turns it on and returns to Manage | Passed |
| US-MG-06 | Enable two-factor without the verification token is rejected | Passed |
| US-SF-01 | A signed-in user sees their full name, Change Password, and Log out | Passed |
| US-SF-01 | An anonymous visitor sees Register and Log in in the navbar | Passed |
| US-SF-01 | Every page shows the brand, the page title, and the footer | Passed |
| US-SF-01 | Pages load the legacy stylesheets | Passed |
| US-SF-02 | The Home URL does not exist | Passed |
| US-SF-02 | The root URL shows the checklist list | Passed |
| US-SF-03 | A flow that returns the Error view shows the error text | Passed |
| US-SF-03 | The error page uses the layout | Passed |
| US-TK-01 | A checked task shows a checked box | Passed |
| US-TK-01 | A checklist with no tasks shows the empty message | Passed |
| US-TK-01 | A soft-deleted task is hidden | Passed |
| US-TK-01 | An anonymous visit is sent to login | Passed |
| US-TK-01 | An unknown checklist is not found | Passed |
| US-TK-01 | Another user is sent to login | Passed |
| US-TK-01 | Each task has edit and delete controls | Passed |
| US-TK-01 | The author sees the checklist name and its tasks | Passed |
| US-TK-02 | Another user cannot open the new task form | Passed |
| US-TK-02 | New task for an unknown checklist is not found | Passed |
| US-TK-02 | Save a new task and the task list shows it | Passed |
| US-TK-02 | Save task without the verification token is rejected | Passed |
| US-TK-02 | Save with a title longer than 255 characters shows the length message | Passed |
| US-TK-02 | Save with an empty title shows the required message | Passed |
| US-TK-02 | The new task form shows an empty title for this checklist | Passed |
| US-TK-03 | Another user cannot open the task rename form | Passed |
| US-TK-03 | Edit of a soft-deleted task is not found | Passed |
| US-TK-03 | Edit of a task in a different checklist id is not found | Passed |
| US-TK-03 | Save of an unknown task is not found | Passed |
| US-TK-03 | The author opens the task rename form | Passed |
| US-TK-03 | The author saves a new task title | Passed |
| US-TK-04 | An anonymous visitor can add a task through Save | Passed |
| US-TK-04 | Another signed-in user can add a task to someone else's checklist | Passed |
| US-TK-05 | An anonymous task delete is rejected | Passed |
| US-TK-05 | Another user cannot delete the task | Passed |
| US-TK-05 | Delete of a missing task is not found | Passed |
| US-TK-05 | Delete of an already deleted task succeeds again | Passed |
| US-TK-05 | The author deletes a task and it leaves the list | Passed |
| US-TK-05 | The task list carries the legacy delete and check scripts | Passed |
| US-TK-06 | Another user cannot check the task | Passed |
| US-TK-06 | Checking a deleted task is a bad request | Passed |
| US-TK-06 | Checking a missing task is not found | Passed |
| US-TK-06 | Checking a task marks it checked | Passed |
| US-TK-06 | Checking a task that is already checked keeps it checked | Passed |
| US-TK-07 | Another user cannot uncheck the task | Passed |
| US-TK-07 | Unchecking a deleted task is a bad request | Passed |
| US-TK-07 | Unchecking a missing task is not found | Passed |
| US-TK-07 | Unchecking a task clears the check | Passed |
| US-XD-01 | A checklist created by one account is not on another account's list | Passed |
| US-XD-01 | Register, create a checklist, and see it on the list | Passed |
| US-XD-02 | Create a checklist, follow its link, and add a task | Passed |
| US-XD-02 | Renaming a checklist changes the task page heading | Passed |
| US-XD-03 | Delete a checklist and its task page still opens by URL | Passed |
| US-XD-04 | A user who creates a checklist can work its tasks and another user cannot | Passed |
| US-XD-05 | After changing the password the navbar still shows the full name | Passed |
