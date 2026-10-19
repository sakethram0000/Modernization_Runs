# Implementation plan — Checklists

## Module
Checklists. MVC `Index`, `New`, `Save`, `Edit`, and API `Delete`. A minimal `Account/Login` page is included because both checklist controllers use `[Authorize]` and the legacy login path is `/Account/Login`.

## Target project structure
```
runs/ChecklistsWebApp/modernized/
  ChecklistsModern/                 web project, net10.0
    Controllers/ChecklistsController.cs
    Controllers/Apis/ChecklistsController.cs
    Controllers/AccountController.cs
    Views/Checklists/Index.cshtml
    Views/Checklists/ChecklistForm.cshtml
    Views/Account/Login.cshtml
    Views/Shared/_Layout.cshtml
    Views/Shared/_LoginPartial.cshtml
    wwwroot/                         bootstrap.css, bootstrap-paper.css, site.css, checklist logo, jquery, bootstrap.js, bootbox.js
  ChecklistsModern.Tests/            sibling xUnit project
```

## Build order
1. Data layer — EF Core `Checklist` with `Id`, `Name`, `DateAdded`, `IsDeleted`, `AuthorId`. Identity user with `FullName` and the `FullName` claim. SQLite database from the Persistence section.
2. Service layer — list by author and not deleted, get by id with no deleted filter, add with the current user id, rename, soft delete. Same branches as the legacy controller.
3. Controllers — MVC names and the API `Delete` name stay. `Login` GET renders the login form. `Login` POST signs in the seeded user so the preview and the tests can open the checklist pages.
4. Views — copy the checklist headings, the form, the list script text, and the layout chrome. Link the stylesheets from `wwwroot`.

## Technology mapping applied
- `v4.5.2` MVC 5 and Web API 2 become ASP.NET Core MVC on `net10.0`.
- EF 6 becomes EF Core. The legacy LocalDB file is not opened and is not modified.
- Ninject becomes built-in DI. `DbContext` is injected.
- OWIN cookie authentication becomes ASP.NET Core cookie authentication. Login path stays `/Account/Login`. No HTTPS redirect, matching the legacy filters.
- Newtonsoft stays out. This slice has no JSON body.
- Checklist pages and `Delete` stay authorized. A signed-in user who is not the author gets 401 on Edit and Delete. An anonymous page request redirects to login. An anonymous delete returns 401.

## Persistence
Real database: SQLite file `checklists.db` for the preview, created by EF Core in the web project. Tests use a separate SQLite file per test run. This is not `Classification: prototype`.

The legacy database has no seed users. The preview inserts these rows only when its own SQLite file is created:

| User | Full name | Password | Checklist |
|---|---|---|---|
| author@checklists.local | Alex Author | Preview123! | `Packing list` (id 1, not deleted), `Old list` (deleted) |
| other@checklists.local | Other User | Preview123! | `Other list` (not deleted) |
| empty@checklists.local | Empty User | Preview123! | none |

These rows are preview seed data. They are not rows from the legacy database.

The running preview signs in `author@checklists.local` for every request except `/Account/Login`, because the screen checker cannot submit the login form. Tests turn that sign-in off and sign in through the login POST.

## Data migration notes
None required. The legacy LocalDB file is not attached. No script writes to it.

## Open questions / ambiguous behavior
1. `Save` updates `Name` for any existing id and does not check `AuthorId`. `Edit` does check the author. The backlog keeps that split. Approve it, or tell me to reject a different user on Save.
2. `GetChecklist` returns a soft-deleted row. Rename still works, and the row stays off the list and stays deleted. Approve keeping that.
3. Approve SQLite for this slice. The legacy store is LocalDB. This plan does not install or attach that file.
4. Approve the minimal login page and the three seeded users. Register, Manage, and Log off stay unbuilt. `Change Password` and `Log out` remain visible.
5. Approve preview sign-in for the screen checker only. Tests still prove the anonymous redirect, the wrong-author 401, and a saved row read on a later request.
6. The checklist name still points at the task list. That task page will not exist. Approve leaving the link in place.
7. Both legacy files are named `ChecklistsController.cs`. The screen script compares the first file name it finds. API `Delete` is specified here and must be proven by the xUnit scenarios, not by that script.

## Out of scope for this run
Tasks MVC and API, Account actions other than Login, Manage, Application Insights, and external login providers.
