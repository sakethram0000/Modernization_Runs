# Scope proposal — ChecklistsWebApp

First slice only. UI recommendation: ASP.NET Core MVC on `net10.0`. Same controller names, action names, URLs, and screen fields. A React rewrite happens only if the reply includes `use spa`.

## Ranked candidates

### 1. Checklists — recommended

MVC `ChecklistsController` (Index, New, Save, Edit) and API `ChecklistsController.Delete`. The home page is the checklist list. The class requires a signed-in user. Delete is a soft delete through `/api/checklists/{id}`.

This is the smallest slice that still has a screen, an authorization rule, and an API. Tasks stay out of the slice, but a checklist row is what a task points at.

Blast radius: one shared database context. The task screens in the old app read the same checklist author.

### 2. Tasks

MVC `TasksController` and API check, uncheck, and delete. The behavior is the checklist product’s second half. It is a weaker first slice because every task screen loads the parent checklist and checks the author. Building tasks alone means also standing up checklist rows.

### 3. Accounts

`AccountController`, `ManageController`, and ASP.NET Identity. This is the largest template area. It is a poor first proof. External login providers in this repo are commented out.

## Dependencies of the recommended module

- The signed-in user id is stored as `Checklist.AuthorId`. Checklist MVC pages use `[Authorize]`. The delete API returns unauthorized when the author does not match.
- `ApplicationDbContext` also holds tasks and Identity users. This slice uses the checklist set only.
- The task module, which is not in this slice, reads checklist rows by id and author.

## Explicit exclusions for this run

- Tasks MVC: Index, New, Save, Edit
- Tasks API: Delete, Check, Uncheck
- Account and Manage
- Application Insights
- External login providers

These exclusions apply if Checklists is the chosen module. Choosing Tasks or Accounts replaces this list.
