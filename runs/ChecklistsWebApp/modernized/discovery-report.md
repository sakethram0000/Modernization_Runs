# Discovery report — ChecklistsWebApp

Source: https://github.com/ranonm/ChecklistsWebApp
Clone (read-only): `runs/ChecklistsWebApp/legacy`
Inventory: `agent-config/scripts/inventory.ps1`

No stop-list match. One C# ASP.NET MVC 5 web application. Not a Web Site, not VB, not desktop, and not SDK-style.

## Detected framework

| Project | TargetFrameworkVersion | SDK-style |
|---|---|---|
| `Checklists/Checklists.csproj` | `v4.5.2` | no |

`Web.config` compilation and `httpRuntime` also say `4.5.2`. MVC 5.2.3. Web API 5.2.3. Entity Framework 6.1.3. ASP.NET Identity 2.2.1.

Counts: 1 csproj, 0 aspx, 6 controllers, 0 svc, 0 asmx, 0 edmx.

## Project structure

One web project, namespace `Checklists`.

- `Controllers/` — MVC: Checklists, Tasks, Account, Manage
- `Controllers/Apis/` — Web API: Checklists, Tasks
- `Core/Models` — Checklist, Task, ApplicationUser
- `Persistence` — `ApplicationDbContext`, repositories, unit of work
- `Views/` — Razor. Layout brand is Checklists
- Ninject wires `IUnitOfWork` into the controllers

## Authentication

- `<authentication mode="None" />`. Sign-in is OWIN cookie authentication. Login path is `/Account/Login`. The cookie identity is rechecked every 30 minutes.
- Two-factor cookies are registered. Microsoft, Twitter, Facebook, and Google login calls are commented out. No app secrets are present.
- There is no global `Authorize` filter and no `RequireHttps` filter. `FilterConfig` only adds `HandleError`.
- `ChecklistsController` (MVC) has `[Authorize]` on the class.
- `TasksController` (MVC) has no class-level `[Authorize]`. Index, New, and Edit return unauthorized when the checklist author is not the current user. Save does not check the author before adding a task.
- Both API controllers have `[Authorize]`. Delete, check, and uncheck also return unauthorized when the author does not match.
- Account login and register actions are the anonymous Identity screens. Manage is the signed-in account screen.

## Database

- Connection name `DefaultConnection`. LocalDB `MSSQLLocalDB`, database file under `|DataDirectory|`, integrated security. No password in the file.
- `ApplicationDbContext : IdentityDbContext<ApplicationUser>` uses that connection.
- `DbSet<Checklist>` and `DbSet<Task>`.
- No `.edmx`. No stored procedures were found in the controllers or the context.
- Checklist fields: Name (required, 255), DateAdded, IsDeleted, AuthorId. `Delete()` sets `IsDeleted = true`.
- Task fields: Title (required, 255), Checked, DateAdded, ChecklistId, IsDeleted. `Delete()` sets `IsDeleted`. `Check()` and `Uncheck()` flip `Checked`.

## APIs

Default route `api/{controller}/{id}`.

| Controller | Action | Verb | Route |
|---|---|---|---|
| `Apis/ChecklistsController` | Delete | DELETE | `/api/checklists/{id}` |
| `Apis/TasksController` | Delete | DELETE | `/api/tasks/{id}` |
| `Apis/TasksController` | Check | PUT | `/api/tasks/{id}/check` |
| `Apis/TasksController` | Uncheck | DELETE | `/api/tasks/{id}/check` |

Delete is a soft delete. Check and uncheck reject a deleted task with `Cannot check a deleted item` or `Cannot uncheck a deleted item`. A missing id on check or uncheck returns `Requires an id for the to-do item`.

## MVC actions

Default route `{controller}/{action}/{id}`, default controller `Checklists`, action `Index`.

| Controller | Actions |
|---|---|
| ChecklistsController | Index, New, Save (POST), Edit |
| TasksController | Index `checklists/{checklistId}/tasks`, New `checklists/{checklistId}/tasks/new`, Save (POST), Edit `checklists/{checklistId}/tasks/edit/{id}` |
| AccountController | Login, VerifyCode, Register, ConfirmEmail, ForgotPassword, ForgotPasswordConfirmation, ResetPassword, ResetPasswordConfirmation, ExternalLogin, SendCode, ExternalLoginCallback, ExternalLoginConfirmation, LogOff, ExternalLoginFailure |
| ManageController | Index, RemoveLogin, AddPhoneNumber, EnableTwoFactorAuthentication, DisableTwoFactorAuthentication, VerifyPhoneNumber, RemovePhoneNumber, ChangePassword, SetPassword, ManageLogins, LinkLogin, LinkLoginCallback |

## Configuration

- Session element was not set in `Web.config`. No `Session[...]` use was found in the controllers read here.
- No `HttpContext.Current` in the controllers read here.
- Application Insights HTTP module is registered.
- Client validation and unobtrusive JavaScript are enabled.
- Layout title suffix is `Checklists`. Navbar brand is Checklists. Footer is `Checklists`. Styles come from `~/Content/css`.

## Dependencies

Auth: Microsoft.AspNet.Identity.Core 2.2.1, EntityFramework Identity 2.2.1, Identity.Owin 2.2.1, Owin 3.0.1, cookie and external-login packages 3.0.1.
Data: EntityFramework 6.1.3.
API and JSON: Microsoft.AspNet.WebApi 5.2.3, Newtonsoft.Json 6.0.4.
Web: Microsoft.AspNet.Mvc 5.2.3.
DI: Ninject 3.2.0.0, Ninject.MVC5 3.2.1.0, Ninject.Web.WebApi 3.2.4.0.
Other packages from the inventory script: Antlr 3.4.1.9004, bootbox 4.3.0, bootstrap 3.0.0, jQuery 1.10.2, jQuery.Validation 1.11.1, Application Insights 2.2.0 family, Microsoft.Net.Compilers 1.3.2, Modernizr 2.6.2, Respond 1.2.0, WebActivatorEx 2.0, WebGrease 1.5.2.

## Call graph

```
Checklists/Index, New, Save, Edit  → ChecklistRepository → ApplicationDbContext
Tasks/Index, New, Save, Edit       → ChecklistRepository and TaskRepository
DELETE /api/checklists/{id}        → Checklist.Delete()
DELETE /api/tasks/{id}             → Task.Delete()
PUT /api/tasks/{id}/check          → Task.Check()
DELETE /api/tasks/{id}/check       → Task.Uncheck()
Account and Manage                 → ASP.NET Identity
```

A task always belongs to a checklist. Both screens decide access from `AuthorId`.

## Services

No WCF and no ASMX. The Web API above is the only service surface. The Razor pages are the consumer.

## Open questions

1. MVC `TasksController.Save` adds a new task without checking that the signed-in user owns the checklist.
2. MVC Tasks has no `[Authorize]`. The owner check returns unauthorized only after a checklist is loaded.
3. Soft-deleted checklists are hidden from the delete API by `IsDeleted`, but the list filter was not fully traced in this pass. Phase 4 must read `GetChecklistsCreatedByUser` before coding.
