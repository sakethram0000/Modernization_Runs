# Test report — Checklists

Authorization is in scope. The anonymous redirect, the anonymous delete, and the wrong-author edit and delete have passing tests.

Real persistence is in scope. This is not `Classification: prototype`. `Save a new checklist and the next request still shows it` reads the new row on a later request from SQLite.

The delete API is in scope. Those tests call `DELETE /api/checklists/{id}` and check the status code and the response body.

## Commands

`dotnet test runs/ChecklistsWebApp/modernized/ChecklistsModern.Tests/ChecklistsModern.Tests.csproj --nologo`

```
Determining projects to restore...
All projects are up-to-date for restore.
ChecklistsModern -> ChecklistsModern.dll
ChecklistsModern.Tests -> ChecklistsModern.Tests.dll
Test run for ChecklistsModern.Tests.dll (.NETCoreApp,Version=v10.0)
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    24, Skipped:     0, Total:    24, Duration: 6 s - ChecklistsModern.Tests.dll (net10.0)
```

`compare-parity.ps1` against `http://127.0.0.1:5288` printed `=== PARITY PASSED ===`. Stylesheets loaded: `bootstrap.css` (127317 bytes), `bootstrap-paper.css` (179125 bytes), `site.css` (2660 bytes).

The screen script compared `ChecklistsController.cs` and found `Index`, `New`, `Save`, and `Edit`. The API file uses `IHttpActionResult` in the legacy project, so that comparison does not list `Delete`. The delete scenarios cover it.

## Scenarios

Every `Scenario:` title is the xUnit display name. All 24 passed.

| Scenario | Test |
|---|---|
| Signed-in user sees only their own checklists | passed |
| A soft-deleted checklist is hidden from the list | passed |
| A user with no checklists sees the empty message | passed |
| New checklist form shows an empty name | passed |
| Save a new checklist and the next request still shows it | passed |
| Save with an empty name shows the required message | passed |
| Save with a name longer than 255 characters shows the length message | passed |
| Save without the verification token is rejected | passed |
| The author opens the rename form | passed |
| Edit without an id is not found | passed |
| Edit of an unknown id is not found | passed |
| A different signed-in user cannot open the rename form | passed |
| The author saves a new name | passed |
| Save of an unknown id is not found and does not create a checklist | passed |
| Save of a soft-deleted checklist changes the name and leaves it deleted | passed |
| A different signed-in user can change the name through Save | passed |
| The author deletes a checklist and it leaves the list | passed |
| Delete of a missing checklist is not found | passed |
| Delete of an already deleted checklist is not found | passed |
| A different signed-in user cannot delete the checklist | passed |
| An anonymous delete is rejected | passed |
| An anonymous visit to the checklist list is sent to login | passed |
| The login page shows the email and password fields | passed |
| A signed-in checklist page shows the author header | passed |

No scenario is missing a passing test.
