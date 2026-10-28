# Scope proposal — ChecklistsWebApp

Scope: whole application

Gate 2 reply: `approved: whole app`. UI: ASP.NET Core MVC (no `use spa`).

UI recommendation for every option: ASP.NET Core MVC on `net10.0`, with the same controller names, action names, URLs, and screen fields. A React rewrite happens only if the reply includes `use spa`. A SPA is a redesign and breaks the parity contract unless you accept that.

Counts below come from `inventory.ps1` and the view folders in `discovery-report.md`.

## Ranked module candidates

### 1. Checklists

MVC `ChecklistsController` (`Index`, `New`, `Save`, `Edit`) and API `ChecklistsController.Delete`. 5 actions, 2 views. The list is the home page at `/`.

Smallest slice with a screen, an authorization rule, and an API. It still needs sign-in, so a minimal login has to be built with it.

Blast radius: the shared database context and the `AuthorId` rule that the task screens also use.

### 2. Tasks

MVC `TasksController` (`Index`, `New`, `Save`, `Edit`) and API `TasksController` (`Delete`, `Check`, `Uncheck`). 7 actions, 2 views.

The daily-use part of the app. A weak standalone slice: every task screen loads the parent checklist and checks its author, so the checklist table, the author rule, and sign-in have to exist first.

Blast radius: carries the legacy security gaps (no `[Authorize]` on the MVC controller, no ownership check on `Save`).

### 3. Accounts

`AccountController` (14 actions) and `ManageController` (12 actions). 18 views including `Shared/Lockout`, `_LoginPartial`, and `_ExternalLoginsListPartial`.

The biggest area, and mostly the Identity project template. Many flows depend on email, SMS, or external providers that the legacy app never configured. A poor first proof on its own, because no business screen would be visible.

Blast radius: every other domain depends on sign-in and the `FullName` claim.

## Whole application

Everything in one pass through Phases 4–7, with the same four gates.

| Item | Count |
|---|---|
| Controllers | 6 (4 MVC, 2 Web API) |
| Actions | 38 (Checklists 5, Tasks 7, Accounts 26) |
| Views | 26 (Account 12, Manage 6, Checklists 2, Tasks 2, Shared 4) plus `_ViewStart` |
| `.aspx` pages | 0 |
| `.svc` / `.asmx` services | 0 |
| Database tables | `Checklists`, `Tasks`, and the five Identity tables |

**Shared foundation, built first:**
- One EF Core data context with `Checklists`, `Tasks`, and Identity users including `FullName`
- ASP.NET Core Identity with the legacy user, password, and lockout settings, and the `FullName` claim
- All Account and Manage screens
- `_Layout` and `_LoginPartial` (brand with logo, `Hello <FullName>!`, Change Password, Log out, Register, Log in, footer), `Error`, `Lockout`
- Default route so `/` is the checklist list; Bootstrap 3 paper theme, `site.css`, jQuery, bootbox under `wwwroot`

There is no Home page to build. The legacy repo has no `HomeController`.

**Then the domains, in dependency order:** Checklists, then Tasks.

**Legacy behavior that depends on configuration the repo does not contain.** Proposed handling: port the actions and keep the legacy behavior without that configuration. Nothing is invented.

| Area | Legacy behavior today | Proposed handling |
|---|---|---|
| Email service (`ForgotPassword`, `ConfirmEmail`, `ResetPassword`, `Email Code` two-factor) | `EmailService.SendAsync` sends nothing | Same no-op sender behind an interface, with empty SMTP settings in `appsettings.json` |
| SMS service (`AddPhoneNumber`, `VerifyPhoneNumber`, `Phone Code` two-factor) | `SmsService.SendAsync` sends nothing | Same no-op sender, with empty SMS settings |
| External logins (`ExternalLogin`, `ExternalLoginCallback`, `ExternalLoginConfirmation`, `ExternalLoginFailure`, `ManageLogins`, `LinkLogin`, `LinkLoginCallback`, `RemoveLogin`) | No provider registered; screens say "There are no external authentication services configured" | Port the actions and screens with no provider registered; empty provider settings |

**Proposed out-of-scope rows (you confirm them at Gate 3):**

| Item | Reason |
|---|---|
| Application Insights (HTTP module, `ApplicationInsights.config`) | Telemetry with no instrumentation key in the repo. It is not a controller action or a screen. Alternative: add the current Application Insights SDK with an empty connection string |
| `Properties/PublishProfiles/ChecklistsApp - Web Deploy.pubxml` | Azure deploy profile containing a real password. Deployment is not part of this run and the secret must not be copied |
| `Web.Debug.config` transform | Build transform with no .NET 10 equivalent; environment settings move to `appsettings.Development.json` |

**Risk:** the Gate 3 files cover all three domains, so they are longer to review. Accounts alone is 26 actions.

## Dependencies of the recommended option

These are the cross-domain edges in `knowledge-graph.md`. Each needs a test in Phase 6 that goes through both domains.

1. Checklists → Accounts: `AuthorId` is the signed-in user (required FK, cascade delete).
2. Checklists → Accounts: `[Authorize]` on both checklist controllers, plus the author check on `Edit` and API `Delete`.
3. Tasks → Checklists: `ChecklistId` FK, and the task screens load the parent checklist and show its name.
4. Tasks → Checklists → Accounts: task access is allowed only for the parent checklist's author.
5. Checklists → Tasks: each checklist name links to `/checklists/{id}/tasks`.
6. Shared layout → Accounts: navbar shows the `FullName` claim and the login and logout links.

## Explicit exclusions for this run

For the whole-application option: only the three proposed out-of-scope rows above.

If you choose a single module instead, the exclusions are every other domain. For example, choosing Checklists excludes all 7 Tasks actions and 25 of the 26 Accounts actions (only `Login` is built).

## Recommendation

**Whole application.** The app is small (38 actions), the three domains share one data context, and Tasks cannot be used without Checklists and sign-in. A module-only run would build most of the foundation anyway and still leave the main daily flow, Tasks, unusable. This is a recommendation, not a decision.

## How to reply

- `approved: whole app` for the whole application
- `approved: Checklists`, `approved: Tasks`, or `approved: Accounts` for one module
- Add `use spa` only if you want a React rewrite instead of the same screens

After your reply, this file's second line becomes `Scope: whole application` or `Scope: module`.
