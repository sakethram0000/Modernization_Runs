# Discovery report — ChecklistsWebApp

Source: https://github.com/ranonm/ChecklistsWebApp, branch `master`, commit `7a4225a34f87cc266781749adc75b7a2ef5fa4b1` (2017-05-15, "Prep for release"). Shallow clone in `runs/ChecklistsWebApp/legacy`. Read-only.

Discovery covers the whole repository.

## Detected version

From `inventory.ps1`:

| Project | TargetFrameworkVersion | SDK-style |
|---|---|---|
| `Checklists/Checklists.csproj` | `v4.5.2` | no |

One project. One solution file, `Checklists.sln`.

## Inventory counts

From `inventory.ps1`:

| Item | Count |
|---|---|
| csproj | 1 |
| controllers | 6 |
| actions | 38 |
| `.aspx` pages | 0 |
| `.svc` services | 0 |
| `.asmx` services | 0 |
| `.edmx` models | 0 |

## Controller and action list

Copied from `inventory.ps1`. The script lists method names once, so a GET and a POST with the same name are one entry. The verb column below comes from reading the code.

| File | Type | Actions |
|---|---|---|
| `Checklists/Controllers/ChecklistsController.cs` | MVC | `Index`, `New`, `Save` (POST), `Edit` |
| `Checklists/Controllers/TasksController.cs` | MVC | `Index`, `New`, `Save` (POST), `Edit` |
| `Checklists/Controllers/Apis/ChecklistsController.cs` | Web API | `Delete` |
| `Checklists/Controllers/Apis/TasksController.cs` | Web API | `Delete`, `Check`, `Uncheck` |
| `Checklists/Controllers/AccountController.cs` | MVC | `Login` (GET, POST), `VerifyCode` (GET, POST), `Register` (GET, POST), `ConfirmEmail`, `ForgotPassword` (GET, POST), `ForgotPasswordConfirmation`, `ResetPassword` (GET, POST), `ResetPasswordConfirmation`, `ExternalLogin` (POST), `SendCode` (GET, POST), `ExternalLoginCallback`, `ExternalLoginConfirmation` (POST), `LogOff` (POST), `ExternalLoginFailure` |
| `Checklists/Controllers/ManageController.cs` | MVC | `Index`, `RemoveLogin` (POST), `AddPhoneNumber` (GET, POST), `EnableTwoFactorAuthentication` (POST), `DisableTwoFactorAuthentication` (POST), `VerifyPhoneNumber` (GET, POST), `RemovePhoneNumber` (POST), `ChangePassword` (GET, POST), `SetPassword` (GET, POST), `ManageLogins`, `LinkLogin` (POST), `LinkLoginCallback` |

There is no `HomeController`.

## Pages and services

- `.aspx` pages: none
- `.svc` / `.asmx` services: none

## Project structure

ASP.NET MVC 5 with Web API 2 in one web application project.

| Folder | Contents |
|---|---|
| `App_Start/` | `BundleConfig`, `FilterConfig`, `IdentityConfig`, `NinjectWebCommon`, `RouteConfig`, `Startup.Auth`, `WebApiConfig` |
| `Controllers/` | MVC controllers. `Controllers/Apis/` holds the Web API controllers |
| `Core/` | `IUnitOfWork`, models (`ApplicationUser`, `Checklist`, `Task`), repository interfaces, view models, `IdentityExtensions.GetFullName` |
| `Persistence/` | `ApplicationDbContext`, `UnitOfWork`, `ChecklistRepository`, `TaskRepository`, EF6 code-first migrations |
| `Views/` | `Account` (12 views + partial), `Checklists` (2), `Manage` (6), `Tasks` (2), `Shared` (`_Layout`, `_LoginPartial`, `Error`, `Lockout`) |
| `Content/`, `Scripts/` | Bootstrap 3 with the `bootstrap-paper` theme, `site.css`, jQuery 1.10.2, jQuery Validation, bootbox, Modernizr, Respond, `Content/Images/checklist_logo.png` |

## Startup and routing

- `Global.asax.cs` `Application_Start`: registers Web API config, areas, global filters, MVC routes, bundles. No static state and no `Application_BeginRequest`.
- `Startup.cs` (OWIN) calls `ConfigureAuth`.
- `NinjectWebCommon` binds every class in the assembly to its default interfaces by convention (`IUnitOfWork` → `UnitOfWork`). `RegisterServices` is empty.
- MVC routes: attribute routes first, then `{controller}/{action}/{id}` with default `Checklists/Index`. The checklist list is therefore served at `/`.
- MVC attribute routes (all on `TasksController`): `checklists/{checklistId}/tasks` (Index), `checklists/{checklistId}/tasks/new` (New), `checklists/{checklistId}/tasks/edit/{id}` (Edit). `Save` uses the conventional route `/Tasks/Save`.
- Web API routes: attribute routes, then `api/{controller}/{id}`.
- Global MVC filter: `HandleErrorAttribute` only. No `RequireHttps`, no global `Authorize`.
- Bundles: `~/Content/css` = `bootstrap.css`, `bootstrap-paper.css`, `site.css`. Scripts: jquery, jqueryval, modernizr, bootstrap (`bootstrap.js`, `bootbox.js`, `respond.js`).

## Authentication inventory

| Item | Value |
|---|---|
| Mode | ASP.NET Identity 2.2.1 over OWIN cookie authentication. `Web.config` sets `<authentication mode="None" />` and removes `FormsAuthentication` |
| Login path | `/Account/Login` |
| Cookies | Application cookie, external sign-in cookie, two-factor cookie (5 minutes), two-factor remember-browser cookie |
| Security stamp | Revalidated every 30 minutes |
| Claims | `FullName` claim added at sign-in; shown in the navbar as `Hello <FullName>!` |
| User rules | Email is the user name. Unique email required. Non-alphanumeric user names allowed |
| Password rules | At least 6 characters, one non-letter-or-digit, one digit, one lowercase, one uppercase. The register view model also enforces 6–100 characters |
| Lockout | Enabled by default, 5 failures, 5 minutes. Login calls `PasswordSignInAsync(..., shouldLockout: false)`, so failed logins never lock the account |
| Two-factor | `Phone Code` and `Email Code` providers registered. `EmailService` and `SmsService` are empty stubs that send nothing. The Manage page hides the two-factor and phone UI |
| External providers | Microsoft, Twitter, Facebook, Google code is commented out. No provider is active |
| Roles | `AspNetRoles` table exists. No code uses roles |
| Global filters | None for auth. No `RequireHttps` |

Authorization by controller:

| Controller | Attribute | Anonymous actions | Per-row rule |
|---|---|---|---|
| MVC `ChecklistsController` | `[Authorize]` | none | `Edit` returns 401 when `AuthorId` is not the signed-in user. `Save` does not check the author |
| MVC `TasksController` | **none** | all | `Index`, `New`, `Edit` return 401 when the checklist author is not the current user. `Save` does no author check |
| API `ChecklistsController` | `[Authorize]` | none | `Delete` returns 401 when not the author |
| API `TasksController` | `[Authorize]` | none | `Delete`, `Check`, `Uncheck` return 401 when not the checklist author |
| `AccountController` | `[Authorize]` | every action except `LogOff` | — |
| `ManageController` | `[Authorize]` | none | acts on the signed-in user only |

With OWIN cookie authentication, an MVC 401 is turned into a 302 redirect to `/Account/Login?ReturnUrl=...`. Web API 401 responses stay 401.

## Database inventory

| Item | Value |
|---|---|
| ORM | Entity Framework 6.1.3, code first, `IdentityDbContext<ApplicationUser>` |
| Connection name | `DefaultConnection`: SQL Server LocalDB, `AttachDbFilename=|DataDirectory|\aspnet-Checklists-20170423085305.mdf`, Integrated Security. No password |
| Database file | No `.mdf` or `.ldf` file is in the repository. There is no legacy data to read |
| Migrations | 10 migrations under `Persistence/Migrations`, automatic migrations off, empty `Seed` |
| Raw SQL / stored procedures | none |

Schema after the last migration:

| Table | Columns |
|---|---|
| `AspNetUsers` | Identity 2 columns (`Id`, `Email`, `EmailConfirmed`, `PasswordHash`, `SecurityStamp`, `PhoneNumber`, `PhoneNumberConfirmed`, `TwoFactorEnabled`, `LockoutEndDateUtc`, `LockoutEnabled`, `AccessFailedCount`, `UserName`) plus `FullName` (required, 255) |
| `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetUserLogins` | Identity 2 defaults |
| `Checklists` | `Id` (identity), `Name` (required, 255), `DateAdded` (required), `IsDeleted`, `AuthorId` (required, FK to `AspNetUsers`, cascade delete) |
| `Tasks` (renamed from `TodoItems`) | `Id` (identity), `Title` (required, 255), `Checked`, `DateAdded`, `ChecklistId` (FK to `Checklists`, cascade delete), `IsDeleted` |

Repository queries:

| Method | Filter |
|---|---|
| `ChecklistRepository.GetChecklist(id)` | by id. Does **not** filter `IsDeleted` |
| `ChecklistRepository.GetChecklistsCreatedByUser(authorId)` | author matches and not deleted |
| `ChecklistRepository.GetChecklistWithTasks(id)` | by id with tasks. Not called anywhere |
| `ChecklistRepository.AddChecklist(checklist)` | empty method. Not called |
| `TaskRepository.GetTaskFromChecklist(checklistId, taskId)` | id and checklist match and not deleted, includes checklist |
| `TaskRepository.GetTaskWithChecklist(taskId)` | by id, includes checklist. Does **not** filter `IsDeleted` |
| `TaskRepository.GetTasksFromChecklist(checklistId)` | checklist matches and not deleted |

Deletes are soft deletes (`IsDeleted = true`) for both checklists and tasks.

## API inventory

| Controller | Action | Verb | Route | Results |
|---|---|---|---|---|
| `Apis/ChecklistsController` | `Delete(int id)` | DELETE | `/api/checklists/{id}` | 404 if missing or already deleted; 401 if not author; 204 after soft delete |
| `Apis/TasksController` | `Delete(int id)` | DELETE | `/api/tasks/{id}` | 404 if missing; 401 if not author; 204 after soft delete. Deleting an already deleted task returns 204 again |
| `Apis/TasksController` | `Check(int? id)` | PUT | `/api/tasks/{id}/check` | 400 `"Requires an id for the to-do item"`; 404; 401; 400 `"Cannot check a deleted item"`; 200 with no body |
| `Apis/TasksController` | `Uncheck(int? id)` | DELETE | `/api/tasks/{id}/check` | 400 `"Requires an id for the to-do item"`; 404; 401; 400 `"Cannot uncheck a deleted item"`; 204 |

No WCF or ASMX services.

## Configuration inventory

Secrets removed.

| Source | Key | Value |
|---|---|---|
| `Web.config` | `connectionStrings/DefaultConnection` | LocalDB attach file, Integrated Security. No secret |
| `Web.config` | `appSettings` | `webpages:Version` 3.0.0.0, `webpages:Enabled` false, `ClientValidationEnabled` true, `UnobtrusiveJavaScriptEnabled` true |
| `Web.config` | `system.web` | `authentication mode="None"`, `compilation debug="true"`, `httpRuntime targetFramework="4.5.2"`. No session state settings |
| `Web.config` | modules | `ApplicationInsightsWebTracking` |
| `ApplicationInsights.config` | `InstrumentationKey` | not set |
| `Properties/PublishProfiles/ChecklistsApp - Web Deploy.pubxml` | Azure Web Deploy target and an Azure SQL connection string | **Contains a real user name and password in a public repository.** Not copied into this report and must not be used by this run |

## Dependency inventory

From `inventory.ps1` (`packages.config`). Packages that matter for auth, data access, or JSON are marked.

| Package | Version | Note |
|---|---|---|
| Microsoft.AspNet.Mvc | 5.2.3 | |
| Microsoft.AspNet.WebApi (Client, Core, WebHost) | 5.2.3 | |
| Microsoft.AspNet.Razor / WebPages | 3.2.3 | |
| Microsoft.AspNet.Web.Optimization | 1.1.3 | bundling; no .NET 10 version |
| EntityFramework | 6.1.3 | **data access** |
| Microsoft.AspNet.Identity.Core / EntityFramework / Owin | 2.2.1 | **auth** |
| Microsoft.Owin, .Host.SystemWeb, .Security, .Security.Cookies, .Security.OAuth | 3.0.1 | **auth** |
| Microsoft.Owin.Security.Facebook / Google / MicrosoftAccount / Twitter | 3.0.1 | **auth**, referenced but not enabled |
| Owin | 1.0 | |
| Newtonsoft.Json | 6.0.4 | **JSON**, used by Web API 2 only; no direct use in app code |
| Ninject, Ninject.Extensions.Conventions, Ninject.MVC5, Ninject.Web.Common(.WebHost), Ninject.Web.WebApi(.WebHost) | 3.2.x | DI |
| WebActivatorEx | 2.0 | starts Ninject |
| Microsoft.ApplicationInsights (+ Agent.Intercept, DependencyCollector, PerfCounterCollector, Web, WindowsServer, TelemetryChannel) | 2.2.0 / 2.0.6 | telemetry, no instrumentation key |
| Microsoft.CodeDom.Providers.DotNetCompilerPlatform | 1.0.3 | |
| Microsoft.Net.Compilers | 1.3.2 | |
| Microsoft.Web.Infrastructure | 1.0.0.0 | |
| Antlr | 3.4.1.9004 | bundling dependency |
| WebGrease | 1.5.2 | bundling dependency |
| bootstrap | 3.0.0 | |
| bootbox | 4.3.0 | |
| jQuery | 1.10.2 | |
| jQuery.Validation | 1.11.1 | |
| Microsoft.jQuery.Unobtrusive.Validation | 3.2.3 | |
| Modernizr | 2.6.2 | |
| Respond | 1.2.0 | |

## Service inventory

None. Business logic lives in the controllers, the two repositories, and the `Checklist` and `Task` model methods (`Delete`, `Check`, `Uncheck`).

## Call graph summary

| Entry point | Calls |
|---|---|
| MVC `Checklists.Index` | `ChecklistRepository.GetChecklistsCreatedByUser` |
| MVC `Checklists.New` | — |
| MVC `Checklists.Save` | `ChecklistRepository.Add` or `GetChecklist`, `UnitOfWork.Complete` |
| MVC `Checklists.Edit` | `ChecklistRepository.GetChecklist` |
| MVC `Tasks.Index` | `ChecklistRepository.GetChecklist`, `TaskRepository.GetTasksFromChecklist` |
| MVC `Tasks.New` | `ChecklistRepository.GetChecklist` |
| MVC `Tasks.Save` | `TaskRepository.Add` or `GetTaskFromChecklist`, `UnitOfWork.Complete` |
| MVC `Tasks.Edit` | `TaskRepository.GetTaskFromChecklist` |
| API `Checklists.Delete` | `ChecklistRepository.GetChecklist`, `Checklist.Delete`, `Complete` |
| API `Tasks.Delete` / `Check` / `Uncheck` | `TaskRepository.GetTaskWithChecklist`, `Task.Delete` / `Check` / `Uncheck`, `Complete` |
| `Account.*`, `Manage.*` | `ApplicationUserManager`, `ApplicationSignInManager`, OWIN authentication manager |
| `Views/Checklists/Index` (script) | `DELETE api/checklists/{id}` |
| `Views/Tasks/Index` (script) | `PUT` / `DELETE /api/tasks/{id}/check`, `DELETE /api/tasks/{id}` |

Fan-in: `ChecklistRepository.GetChecklist` is used by both checklist and task screens. `ApplicationDbContext` is shared by every domain. The checklist `AuthorId` is the ownership rule for both checklists and tasks.

## Technology profile (confirmed)

| Layer | Found | Target |
|---|---|---|
| Runtime | .NET Framework 4.5.2, IIS / IIS Express | `net10.0` |
| Web UI | ASP.NET MVC 5 Razor views | ASP.NET Core MVC, same controllers, actions, URLs, and fields |
| APIs | Web API 2 (`ApiController`, `IHttpActionResult`) | ASP.NET Core controllers, same routes and status codes |
| Services | none | — |
| Data | EF6 code first, SQL Server LocalDB | EF Core |
| Auth | ASP.NET Identity 2 + OWIN cookies | ASP.NET Core Identity + cookie authentication |
| DI | Ninject by convention | built-in DI |
| Config | `Web.config` | `appsettings.json` + `IOptions<T>` |
| Bundling | System.Web.Optimization | static files under `wwwroot` |
| Telemetry | Application Insights 2.2 (no key) | decision needed |
| JSON | Newtonsoft through Web API 2 | `System.Text.Json` (no JSON bodies are read or written by app code) |

## Cross-cutting concerns

- No `HttpContext.Current` in app code. `AccountController` and `ManageController` use `HttpContext.GetOwinContext()`.
- No `Session`, no `Cache`, no static mutable state, no background threads or timers, no Windows-only APIs.
- `HandleErrorAttribute` renders `Views/Shared/Error.cshtml` on unhandled MVC exceptions when custom errors are on.
- `DateTime.Now` (server local time) sets `DateAdded` in the model constructors.

## Human decision

Stop-list check: no stop item matched. There is a `.csproj`, the code is C#, it is not desktop, not SDK-style, has no WCF, COM, Remoting, or GAC dependency, and no shared library is consumed by another .NET Framework app.

Packages without a `net10.0` build (System.Web.Optimization, Ninject.Web.*, WebActivatorEx, Microsoft.Owin.*, Application Insights 2.2 Web) are replaced by the built-in ASP.NET Core equivalents in the mapping above. Application Insights needs a decision: drop it, or map it to the current Application Insights SDK with an empty connection string.

## Open questions and red flags

1. **Public secret.** The publish profile contains an Azure SQL user name and password. This run must not use or copy it. The owner should rotate it.
2. **No database file.** The LocalDB `.mdf` is not in the repo, so there is no legacy data. The schema comes from the migrations.
3. **MVC `TasksController` has no `[Authorize]`.** An anonymous visitor reaches the actions; `Index`, `New`, `Edit` then return 401 (redirect to login) because the author check fails.
4. **`Tasks.Save` has no ownership check.** It binds the `Task` entity directly. A caller can add a task to any checklist id, rename another user's task, or post `Checked`, `IsDeleted`, and `DateAdded`. A missing checklist id fails the foreign key and returns an error page.
5. **`Checklists.Save` has no ownership check** on update, and it renames soft-deleted checklists because `GetChecklist` does not filter `IsDeleted`.
6. **Soft-deleted checklists stay reachable.** `Edit` and the task screens load a deleted checklist by id.
7. **API `Tasks.Delete` does not check `IsDeleted`**, so deleting a deleted task returns 204 again. `Check` returns 200 while `Uncheck` returns 204.
8. **No `HomeController`.** `RedirectToLocal` falls back to `/Home/Index`, so a login with no `ReturnUrl` ends on a 404.
9. **External login confirmation** creates a user with no `FullName`, which the schema requires. External providers are disabled, so this path cannot run today.
10. **Email and SMS services send nothing.** Forgot password always ends on the confirmation page (no user has a confirmed email), and the reset link is never sent. Phone and two-factor flows generate codes that are never delivered.
11. **Checklist delete script uses a relative URL** (`api/checklists/{id}`). It works only when the list is opened at `/`. At `/Checklists` or `/Checklists/Index` the request goes to `/Checklists/api/checklists/{id}`.
12. **Task view links may not resolve:** "Create a new to-do item" calls `Url.Action("New", "Tasks")` without `checklistId`, and the task form's Cancel link passes `Id` instead of `ChecklistId`. Both depend on ambient route values.
13. **Markup quirks to keep or fix:** `ChecklistForm.cshtml` prints a stray `;` after the hidden `Id`. Its Cancel link passes the CSS class as a route value, so the link has no button class and a `?class=...` query string.
14. **Lockout is configured but never triggered**, because login uses `shouldLockout: false`.
