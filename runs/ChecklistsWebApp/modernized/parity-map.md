# Parity map — Checklists

## Module
Checklists

## Project type
MVC and Web API

## Target
`net10.0`

## Controllers and actions

| Old file | Old type | Action | Verb | Old URL or page | New file | New action | New URL |
|---|---|---|---|---|---|---|---|
| `Controllers/ChecklistsController.cs` | MVC | `Index` | GET | `/Checklists` | `Controllers/ChecklistsController.cs` | `Index` | `/Checklists` |
| `Controllers/ChecklistsController.cs` | MVC | `New` | GET | `/Checklists/New` | `Controllers/ChecklistsController.cs` | `New` | `/Checklists/New` |
| `Controllers/ChecklistsController.cs` | MVC | `Save` | POST | `/Checklists/Save` | `Controllers/ChecklistsController.cs` | `Save` | `/Checklists/Save` |
| `Controllers/ChecklistsController.cs` | MVC | `Edit` | GET | `/Checklists/Edit/{id}` | `Controllers/ChecklistsController.cs` | `Edit` | `/Checklists/Edit/{id}` |
| `Controllers/Apis/ChecklistsController.cs` | Web API | `Delete` | DELETE | `/api/checklists/{id}` | `Controllers/Apis/ChecklistsController.cs` | `Delete` | `/api/checklists/{id}` |
| `Account/Login` | MVC | `Login` | GET | `/Account/Login` | `Account/Login` | `Login` | `/Account/Login` |

`Login` is the redirect target for `[Authorize]`. The rest of `AccountController` is not in this map.

The default MVC route uses controller `Checklists` and action `Index`, so the list is also served at `/`.

## Shared chrome

The running preview signs in Alex Author, except on `/Account/Login`. Rows below use the page that actually contains the text.

| Location | Kind | Text that must appear | Page URL |
|---|---|---|---|
| Layout title | title | Checklists | `/` |
| Layout navbar | brand | Checklists | `/` |
| Layout navbar | text | Hello Alex Author! | `/` |
| Layout navbar | link | Change Password | `/` |
| Layout navbar | link | Log out | `/` |
| Layout footer | footer | Checklists | `/` |
| Login navbar | link | Register | `/Account/Login` |
| Login navbar | link | Log in | `/Account/Login` |

## Screen fields

| Screen | Old control | Kind | Text that must appear | Page URL |
|---|---|---|---|---|
| Index | heading | heading | My Checklists | `/` |
| Index | create link | link | Create new list | `/` |
| Index | checklist name | link | Packing list | `/` |
| Index | edit link | link | Edit | `/` |
| Index | delete link | link | Delete | `/` |
| Index | confirm script | text | Are you sure that you want to delete the checklist? | `/` |
| Index | confirm script | button | Yes | `/` |
| Index | confirm script | button | No | `/` |
| New | heading | heading | New checklist | `/Checklists/New` |
| New | name label | label | Name | `/Checklists/New` |
| New | save button | button | Save | `/Checklists/New` |
| New | cancel link | link | Cancel | `/Checklists/New` |
| Edit | heading | heading | Modify checklist name | `/Checklists/Edit/1` |
| Edit | current name | value | Packing list | `/Checklists/Edit/1` |
| Login | heading | heading | Log in | `/Account/Login` |
| Login | email label | label | Email | `/Account/Login` |
| Login | password label | label | Password | `/Account/Login` |
| Login | remember me | label | Remember me? | `/Account/Login` |
| Login | submit | button | Log in | `/Account/Login` |
| Login | register link | link | Create an account | `/Account/Login` |

Empty-list copy `You don't have any checklists.` and `Create a checklist` is not on `/` because the preview user has a checklist. US-001 covers that copy.

Validation text is not on the empty form. US-002 covers `The Name field is required.` and `The field Name must be a string with a maximum length of 255.`

## Out of scope on purpose

- Tasks MVC and the task API
- Account actions other than `Login`
- Manage, including the page behind `Change Password`
- `Log out` posts to `Account/LogOff`, which is not built in this slice
- The checklist name still links to the task list. That task URL is not built
- Application Insights
- External login providers
