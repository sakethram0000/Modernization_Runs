# Implementation plan — ChecklistsWebApp

Scope: whole application

## Module
ChecklistsWebApp, every domain: shared foundation, Accounts (Account and Manage), Checklists, Tasks. 6 controllers, 38 actions, 26 views. UI: ASP.NET Core MVC (Gate 2, no `use spa`).

## Target project structure
```
runs/ChecklistsWebApp/modernized/
  ChecklistsModern.slnx
  ChecklistsModern/                      web project, net10.0, nullable enabled
    Program.cs
    appsettings.json, appsettings.Development.json
    Data/ApplicationDbContext.cs          Identity + Checklists + Tasks
    Data/PreviewSeeder.cs                 preview rows, Development only
    Models/ApplicationUser.cs, Checklist.cs, Task.cs (class name ChecklistTask, table Tasks)
    Identity/AppClaimsPrincipalFactory.cs FullName claim
    Identity/LegacyIdentityErrorDescriber.cs, LegacyPasswordValidator.cs
    Services/IChecklistService.cs, ChecklistService.cs
    Services/ITaskService.cs, TaskService.cs
    Services/NoOpEmailSender.cs, NoOpSmsSender.cs
    Options/EmailOptions.cs, SmsOptions.cs, DatabaseOptions.cs
    Extensions/IdentityExtensions.cs      GetFullName
    ViewModels/                           one file per legacy view model, same property names
    Controllers/AccountController.cs
    Controllers/ManageController.cs
    Controllers/ChecklistsController.cs
    Controllers/TasksController.cs
    Controllers/Apis/ChecklistsController.cs
    Controllers/Apis/TasksController.cs
    Views/Account/*  (12 views + _ExternalLoginsListPartial)
    Views/Manage/*   (6 views)
    Views/Checklists/Index.cshtml, ChecklistForm.cshtml
    Views/Tasks/Index.cshtml, TaskForm.cshtml
    Views/Shared/_Layout.cshtml, _LoginPartial.cshtml, Error.cshtml, Lockout.cshtml
    wwwroot/Content/  bootstrap.css, bootstrap-paper.css, Site.css, Images/checklist_logo.png
    wwwroot/Scripts/  jquery-1.10.2.js, jquery.validate*, bootstrap.js, bootbox.js, respond.js, modernizr-2.6.2.js
  ChecklistsModern.Tests/                 sibling xUnit project
```

## Build order

`dotnet build` must pass after each step before the next one starts.

### 1. Shared foundation
1. Data layer — EF Core `ApplicationDbContext : IdentityDbContext<ApplicationUser>` with `Checklists` and `Tasks` sets. Same table names, column names, lengths, required flags, foreign keys, and cascade deletes as the last legacy migration. `ApplicationUser.FullName` required, 255.
2. Identity — ASP.NET Core Identity with the legacy settings: unique email, any user name characters, password length 6 with non-alphanumeric, digit, lowercase, uppercase; lockout on for new users, 5 attempts, 5 minutes; security stamp check every 30 minutes; login path `/Account/Login`; `FullName` claim. Two-factor token providers named `Phone Code` and `Email Code`, plus the data-protection provider for reset and confirmation codes. No authenticator app provider. Error describer with the Identity 2.2.1 texts. One combined password error, as Identity 2 joins password rule messages with a space.
3. Cookie events — MVC requests that end in 401 redirect to the login page, as OWIN did. Requests under `/api` get 401 with no redirect.
4. Email and SMS — `NoOpEmailSender` and `NoOpSmsSender` behind interfaces; they send nothing, like the legacy `EmailService` and `SmsService`.
5. Layout — `_Layout`, `_LoginPartial`, `Error`, `Lockout`, legacy CSS and JS under `wwwroot`. Title `<page> - Checklists`.
6. Routing — conventional `{controller=Checklists}/{action=Index}/{id?}`, attribute routes for the task screens and the task check API. No HTTPS redirect, no global authorization filter.
7. Error handling — `UseExceptionHandler` to the Error view outside Development, matching `HandleErrorAttribute` for remote users.

### 2. Accounts
1. `AccountController` — all 14 legacy actions with the same verbs, parameters, redirects, and views, including `RedirectToLocal` falling back to `/Home`.
2. `ManageController` — all 12 legacy actions, `ManageMessageId` status texts, the hidden phone and two-factor blocks left out of the Index view as in the legacy markup.
3. Views — the 18 Account and Manage views with the legacy headings (including the trailing period), labels, input names, buttons, and links.

### 3. Checklists
1. `ChecklistService` — list by author and not deleted; get by id with no deleted filter; add with the current user id; rename; soft delete.
2. MVC `ChecklistsController` — `Index`, `New`, `Save`, `Edit` with the legacy branches. API `Controllers/Apis/ChecklistsController.Delete` at `DELETE /api/checklists/{id}`.
3. Views — `Index` (with the delete script unchanged) and `ChecklistForm`.

### 4. Tasks
1. `TaskService` — tasks of a checklist not deleted; task of a checklist by id not deleted; task with checklist by id with no deleted filter; add; rename; soft delete; check; uncheck.
2. MVC `TasksController` with no class-level `[Authorize]` and the three attribute routes. `Save` binds the task the same way the legacy entity binding does. API `Controllers/Apis/TasksController` with `Delete`, `Check` (`PUT api/tasks/{id}/check`), `Uncheck` (`DELETE api/tasks/{id}/check`). Bad-request bodies are `{"Message":"..."}`, the Web API 2 shape.
3. Views — `Index` (with the check, uncheck, and delete scripts unchanged) and `TaskForm`.

## Technology mapping applied
- `v4.5.2` MVC 5 and Web API 2 become ASP.NET Core MVC on `net10.0`. API controllers keep their class names in `Controllers/Apis`.
- EF 6 code-first becomes EF Core. Table and column names stay.
- ASP.NET Identity 2 + OWIN cookies become ASP.NET Core Identity + cookie authentication. Same login path and cookie lifetime rules.
- Ninject convention binding becomes built-in DI. `DbContext` is scoped and injected.
- `Web.config` becomes `appsettings.json` with `IOptions<T>`.
- System.Web.Optimization bundles become plain `<link>` and `<script>` tags to the same files.
- Newtonsoft through Web API 2 becomes `System.Text.Json`. The only JSON written is the bad-request body, with the property name `Message` kept.
- No `HttpContext.Current`. All I/O is async.

## Persistence
Real database, not `Classification: prototype`.

- **This run:** SQLite file `checklists.db` for the preview, created by EF Core. Tests use a separate SQLite file per test class. LocalDB is not installed on this machine.
- **Target:** SQL Server. The provider is chosen by `Database:Provider` (`Sqlite` or `SqlServer`) and the `ConnectionStrings:DefaultConnection` setting. The SQL Server path is configured but **not tested in this run**.
- The legacy LocalDB file is not in the repository and is not opened. No legacy rows exist to read.

Preview rows (Development only). **Data source: preview data is fake.** These are not legacy rows.

| User | Full name | Password | Notes |
|---|---|---|---|
| `author@checklists.local` | Alex Author | `Preview123!` | |
| `other@checklists.local` | Other User | `Preview123!` | |
| `empty@checklists.local` | Empty User | `Preview123!` | no checklists |
| `twofactor@checklists.local` | Tina Twofactor | `Preview123!` | email confirmed, two-factor enabled |

| Checklist | Author | Deleted | Tasks |
|---|---|---|---|
| Packing list (id 1) | Alex Author | no | Passport (unchecked), Charger (checked), Old task (deleted) |
| Old list | Alex Author | yes | none |
| Other list | Other User | no | Other task (unchecked) |

Tests seed the same rows into their own database.

## Configuration and secrets
| Setting | Legacy source | New key | Value in this run |
|---|---|---|---|
| Database connection | `Web.config` `DefaultConnection` (LocalDB, Integrated Security) | `ConnectionStrings:DefaultConnection`, `Database:Provider` | `Data Source=checklists.db`, `Sqlite` |
| Azure SQL connection | `Properties/PublishProfiles/ChecklistsApp - Web Deploy.pubxml` | none | **not copied**; out of scope |
| Email sender | `IdentityConfig.EmailService` (no-op) | `Email:SmtpHost`, `Email:From` | empty |
| SMS sender | `IdentityConfig.SmsService` (no-op) | `Sms:AccountSid`, `Sms:AuthToken`, `Sms:From` | empty |
| External login providers | `Startup.Auth.cs` (commented out) | `Authentication:Google:ClientId`, `ClientSecret` (same for Microsoft, Facebook, Twitter) | empty; no provider registered |
| Application Insights | `ApplicationInsights.config` (no key) | none | out of scope |

## Data migration notes
None in this run. No legacy data exists in the repository, and the legacy database is not attached.

For a later cutover with the client's real database, a separate approved script is needed. ASP.NET Core Identity adds `NormalizedUserName`, `NormalizedEmail`, `ConcurrencyStamp`, and `LockoutEnd` and drops `LockoutEndDateUtc`. Identity 2 password hashes stay valid, because the ASP.NET Core password hasher verifies the version 2 format.

## Open questions / ambiguous behavior

Each item below keeps the legacy behavior unless you say otherwise at Gate 3.

### Shared foundation
1. **Login with no return URL goes to `/Home`, which is a 404** (no `HomeController`). The navbar "Log in" link triggers this. Keep, or send it to `/`?
2. **SQLite in this run, SQL Server configured but untested.** Approve.

### Accounts
3. **Identity library texts.** The duplicate-user, password-rule, `Incorrect password.`, `Invalid token.`, and `User already has a password set.` messages come from the Identity 2.2.1 library, not from this repo. I wrote them from that library's resources. Approve them, or give the texts you see in the running legacy app.
4. **Email and SMS stay no-ops** with empty settings. Approve.
5. **External login with a provider name.** No provider is registered. Legacy challenges a type nobody handles. The new app returns to the login page without signing in, because an unknown scheme would be a 500 in ASP.NET Core. Approve.
6. **Two-factor provider names** stay `Email Code` and `Phone Code`. No authenticator app provider is added. Approve.
7. **Existing users are not migrated** in this run (no data). See Data migration notes. Approve.
8. **API 401 behavior.** The legacy OWIN cookie middleware redirects a non-AJAX 401 to the login page and leaves an AJAX 401 alone. The screens always call the API with jQuery (AJAX). The new app always returns 401 for `/api`, and redirects MVC 401s to login. Approve.

### Manage
9. **Phone and two-factor controls stay hidden** on `/Manage`, as in the legacy markup (commented out). The actions still work by direct URL and POST. Approve.

### Checklists
10. **`Save` does not check the author**, so another signed-in user can rename a checklist by posting its id (`Edit` does check). Keep, or reject a different user on `Save`?
11. **Soft-deleted checklists stay reachable** by id through `Edit`, `Save`, and the task screens. Keep?
12. **List and form quirks kept as-is:** the delete script uses the relative URL `api/checklists/`, so delete works only when the list is opened at `/`; the checklist form prints a stray `;`; its Cancel link is `/?class=btn%20btn-link` with no button class. Keep, or fix?

### Tasks
13. **MVC `TasksController` has no `[Authorize]`.** Anonymous visitors are redirected to login only because the author check fails. Keep, or add `[Authorize]`?
14. **`Tasks.Save` has no owner check and binds the whole task.** Anyone, including an anonymous visitor with a token, can add a task to any checklist or rename any task, and can post `Checked`, `IsDeleted`, and `DateAdded`. Keep (scenarios in US-TK-04), or add the author check and bind only `Id`, `Title`, `ChecklistId`?
15. **`Tasks.Save` with a checklist id that does not exist** fails the foreign key and shows the error page. Keep?
16. **API status codes kept as-is:** deleting an already deleted task returns 204; `Check` returns 200 and `Uncheck` returns 204. Keep?
17. **Task form links.** The legacy Cancel link passes `Id` instead of `ChecklistId`, and the empty-list "Create a new to-do item" link has no checklist id. Both rely on route values from the current URL, so after a failed save the legacy Cancel target is not certain. The new app points Cancel at `/checklists/{ChecklistId}/tasks` and the empty-list link at `/checklists/{ChecklistId}/tasks/new`. Approve.

## Out of scope for this run
Also listed in the out-of-scope table in `parity-map.md`.

| Item | Reason |
|---|---|
| Application Insights (HTTP module, `ApplicationInsights.config`) | Telemetry with no instrumentation key in the repo; not an action or a screen |
| `Properties/PublishProfiles/ChecklistsApp - Web Deploy.pubxml` | Azure deploy profile containing a real password; deployment is not part of this run |
| `Web.Debug.config` | Build transform; environment settings move to `appsettings.Development.json` |

Every controller action from `inventory.ps1` is in scope. None of the 38 actions is excluded.
