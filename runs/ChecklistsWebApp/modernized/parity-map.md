# Parity map (`parity-map.md`)

Produced in Phase 4. Phase 5 must follow these names. Phase 6 compares the code to this map.

Scope: whole application

## Module
ChecklistsWebApp (legacy project `Checklists/Checklists.csproj`, .NET Framework v4.5.2)

## Project type
MVC 5 + Web API 2 in one web project

## Target
`net10.0`

## Controllers and actions

`Old file` is relative to `runs/ChecklistsWebApp/legacy/Checklists`. `New file` is relative to `runs/ChecklistsWebApp/modernized/ChecklistsModern`. 48 rows cover the 38 legacy action names. GET and POST overloads of one action are separate rows.

### Shared foundation

The foundation has no controllers of its own. The legacy app has no `HomeController`. Default route `{controller=Checklists}/{action=Index}/{id?}` makes `/` the Checklists index. Layout, login partial, Error, and Lockout views are in the Shared chrome and Screen fields tables.

### Accounts

| Old file | Old type | Action | Verb | Old URL or page | New file | New action | New URL |
|---|---|---|---|---|---|---|---|
| `Controllers/AccountController.cs` | MVC | `Login` | GET | `/Account/Login?returnUrl=` | `Controllers/AccountController.cs` | `Login` | `/Account/Login?returnUrl=` |
| `Controllers/AccountController.cs` | MVC | `Login` | POST | `/Account/Login` | `Controllers/AccountController.cs` | `Login` | `/Account/Login` |
| `Controllers/AccountController.cs` | MVC | `VerifyCode` | GET | `/Account/VerifyCode?provider=&returnUrl=&rememberMe=` | `Controllers/AccountController.cs` | `VerifyCode` | `/Account/VerifyCode?provider=&returnUrl=&rememberMe=` |
| `Controllers/AccountController.cs` | MVC | `VerifyCode` | POST | `/Account/VerifyCode` | `Controllers/AccountController.cs` | `VerifyCode` | `/Account/VerifyCode` |
| `Controllers/AccountController.cs` | MVC | `Register` | GET | `/Account/Register` | `Controllers/AccountController.cs` | `Register` | `/Account/Register` |
| `Controllers/AccountController.cs` | MVC | `Register` | POST | `/Account/Register` | `Controllers/AccountController.cs` | `Register` | `/Account/Register` |
| `Controllers/AccountController.cs` | MVC | `ConfirmEmail` | GET | `/Account/ConfirmEmail?userId=&code=` | `Controllers/AccountController.cs` | `ConfirmEmail` | `/Account/ConfirmEmail?userId=&code=` |
| `Controllers/AccountController.cs` | MVC | `ForgotPassword` | GET | `/Account/ForgotPassword` | `Controllers/AccountController.cs` | `ForgotPassword` | `/Account/ForgotPassword` |
| `Controllers/AccountController.cs` | MVC | `ForgotPassword` | POST | `/Account/ForgotPassword` | `Controllers/AccountController.cs` | `ForgotPassword` | `/Account/ForgotPassword` |
| `Controllers/AccountController.cs` | MVC | `ForgotPasswordConfirmation` | GET | `/Account/ForgotPasswordConfirmation` | `Controllers/AccountController.cs` | `ForgotPasswordConfirmation` | `/Account/ForgotPasswordConfirmation` |
| `Controllers/AccountController.cs` | MVC | `ResetPassword` | GET | `/Account/ResetPassword?code=` | `Controllers/AccountController.cs` | `ResetPassword` | `/Account/ResetPassword?code=` |
| `Controllers/AccountController.cs` | MVC | `ResetPassword` | POST | `/Account/ResetPassword` | `Controllers/AccountController.cs` | `ResetPassword` | `/Account/ResetPassword` |
| `Controllers/AccountController.cs` | MVC | `ResetPasswordConfirmation` | GET | `/Account/ResetPasswordConfirmation` | `Controllers/AccountController.cs` | `ResetPasswordConfirmation` | `/Account/ResetPasswordConfirmation` |
| `Controllers/AccountController.cs` | MVC | `ExternalLogin` | POST | `/Account/ExternalLogin` | `Controllers/AccountController.cs` | `ExternalLogin` | `/Account/ExternalLogin` |
| `Controllers/AccountController.cs` | MVC | `SendCode` | GET | `/Account/SendCode?returnUrl=&rememberMe=` | `Controllers/AccountController.cs` | `SendCode` | `/Account/SendCode?returnUrl=&rememberMe=` |
| `Controllers/AccountController.cs` | MVC | `SendCode` | POST | `/Account/SendCode` | `Controllers/AccountController.cs` | `SendCode` | `/Account/SendCode` |
| `Controllers/AccountController.cs` | MVC | `ExternalLoginCallback` | GET | `/Account/ExternalLoginCallback?returnUrl=` | `Controllers/AccountController.cs` | `ExternalLoginCallback` | `/Account/ExternalLoginCallback?returnUrl=` |
| `Controllers/AccountController.cs` | MVC | `ExternalLoginConfirmation` | POST | `/Account/ExternalLoginConfirmation` | `Controllers/AccountController.cs` | `ExternalLoginConfirmation` | `/Account/ExternalLoginConfirmation` |
| `Controllers/AccountController.cs` | MVC | `LogOff` | POST | `/Account/LogOff` | `Controllers/AccountController.cs` | `LogOff` | `/Account/LogOff` |
| `Controllers/AccountController.cs` | MVC | `ExternalLoginFailure` | GET | `/Account/ExternalLoginFailure` | `Controllers/AccountController.cs` | `ExternalLoginFailure` | `/Account/ExternalLoginFailure` |
| `Controllers/ManageController.cs` | MVC | `Index` | GET | `/Manage?message=` | `Controllers/ManageController.cs` | `Index` | `/Manage?message=` |
| `Controllers/ManageController.cs` | MVC | `RemoveLogin` | POST | `/Manage/RemoveLogin` | `Controllers/ManageController.cs` | `RemoveLogin` | `/Manage/RemoveLogin` |
| `Controllers/ManageController.cs` | MVC | `AddPhoneNumber` | GET | `/Manage/AddPhoneNumber` | `Controllers/ManageController.cs` | `AddPhoneNumber` | `/Manage/AddPhoneNumber` |
| `Controllers/ManageController.cs` | MVC | `AddPhoneNumber` | POST | `/Manage/AddPhoneNumber` | `Controllers/ManageController.cs` | `AddPhoneNumber` | `/Manage/AddPhoneNumber` |
| `Controllers/ManageController.cs` | MVC | `EnableTwoFactorAuthentication` | POST | `/Manage/EnableTwoFactorAuthentication` | `Controllers/ManageController.cs` | `EnableTwoFactorAuthentication` | `/Manage/EnableTwoFactorAuthentication` |
| `Controllers/ManageController.cs` | MVC | `DisableTwoFactorAuthentication` | POST | `/Manage/DisableTwoFactorAuthentication` | `Controllers/ManageController.cs` | `DisableTwoFactorAuthentication` | `/Manage/DisableTwoFactorAuthentication` |
| `Controllers/ManageController.cs` | MVC | `VerifyPhoneNumber` | GET | `/Manage/VerifyPhoneNumber?phoneNumber=` | `Controllers/ManageController.cs` | `VerifyPhoneNumber` | `/Manage/VerifyPhoneNumber?phoneNumber=` |
| `Controllers/ManageController.cs` | MVC | `VerifyPhoneNumber` | POST | `/Manage/VerifyPhoneNumber` | `Controllers/ManageController.cs` | `VerifyPhoneNumber` | `/Manage/VerifyPhoneNumber` |
| `Controllers/ManageController.cs` | MVC | `RemovePhoneNumber` | POST | `/Manage/RemovePhoneNumber` | `Controllers/ManageController.cs` | `RemovePhoneNumber` | `/Manage/RemovePhoneNumber` |
| `Controllers/ManageController.cs` | MVC | `ChangePassword` | GET | `/Manage/ChangePassword` | `Controllers/ManageController.cs` | `ChangePassword` | `/Manage/ChangePassword` |
| `Controllers/ManageController.cs` | MVC | `ChangePassword` | POST | `/Manage/ChangePassword` | `Controllers/ManageController.cs` | `ChangePassword` | `/Manage/ChangePassword` |
| `Controllers/ManageController.cs` | MVC | `SetPassword` | GET | `/Manage/SetPassword` | `Controllers/ManageController.cs` | `SetPassword` | `/Manage/SetPassword` |
| `Controllers/ManageController.cs` | MVC | `SetPassword` | POST | `/Manage/SetPassword` | `Controllers/ManageController.cs` | `SetPassword` | `/Manage/SetPassword` |
| `Controllers/ManageController.cs` | MVC | `ManageLogins` | GET | `/Manage/ManageLogins?message=` | `Controllers/ManageController.cs` | `ManageLogins` | `/Manage/ManageLogins?message=` |
| `Controllers/ManageController.cs` | MVC | `LinkLogin` | POST | `/Manage/LinkLogin` | `Controllers/ManageController.cs` | `LinkLogin` | `/Manage/LinkLogin` |
| `Controllers/ManageController.cs` | MVC | `LinkLoginCallback` | GET | `/Manage/LinkLoginCallback` | `Controllers/ManageController.cs` | `LinkLoginCallback` | `/Manage/LinkLoginCallback` |

### Checklists

| Old file | Old type | Action | Verb | Old URL or page | New file | New action | New URL |
|---|---|---|---|---|---|---|---|
| `Controllers/ChecklistsController.cs` | MVC | `Index` | GET | `/` and `/Checklists` | `Controllers/ChecklistsController.cs` | `Index` | `/` and `/Checklists` |
| `Controllers/ChecklistsController.cs` | MVC | `New` | GET | `/Checklists/New` | `Controllers/ChecklistsController.cs` | `New` | `/Checklists/New` |
| `Controllers/ChecklistsController.cs` | MVC | `Save` | POST | `/Checklists/Save` | `Controllers/ChecklistsController.cs` | `Save` | `/Checklists/Save` |
| `Controllers/ChecklistsController.cs` | MVC | `Edit` | GET | `/Checklists/Edit/{id}` | `Controllers/ChecklistsController.cs` | `Edit` | `/Checklists/Edit/{id}` |
| `Controllers/Apis/ChecklistsController.cs` | Web API | `Delete` | DELETE | `/api/checklists/{id}` | `Controllers/Apis/ChecklistsController.cs` | `Delete` | `/api/checklists/{id}` |

### Tasks

| Old file | Old type | Action | Verb | Old URL or page | New file | New action | New URL |
|---|---|---|---|---|---|---|---|
| `Controllers/TasksController.cs` | MVC | `Index` | GET | `/checklists/{checklistId}/tasks` | `Controllers/TasksController.cs` | `Index` | `/checklists/{checklistId}/tasks` |
| `Controllers/TasksController.cs` | MVC | `New` | GET | `/checklists/{checklistId}/tasks/new` | `Controllers/TasksController.cs` | `New` | `/checklists/{checklistId}/tasks/new` |
| `Controllers/TasksController.cs` | MVC | `Save` | POST | `/Tasks/Save` | `Controllers/TasksController.cs` | `Save` | `/Tasks/Save` |
| `Controllers/TasksController.cs` | MVC | `Edit` | GET | `/checklists/{checklistId}/tasks/edit/{id}` | `Controllers/TasksController.cs` | `Edit` | `/checklists/{checklistId}/tasks/edit/{id}` |
| `Controllers/Apis/TasksController.cs` | Web API | `Delete` | DELETE | `/api/tasks/{id}` | `Controllers/Apis/TasksController.cs` | `Delete` | `/api/tasks/{id}` |
| `Controllers/Apis/TasksController.cs` | Web API | `Check` | PUT | `/api/tasks/{id}/check` | `Controllers/Apis/TasksController.cs` | `Check` | `/api/tasks/{id}/check` |
| `Controllers/Apis/TasksController.cs` | Web API | `Uncheck` | DELETE | `/api/tasks/{id}/check` | `Controllers/Apis/TasksController.cs` | `Uncheck` | `/api/tasks/{id}/check` |

## Shared chrome

From `Views/Shared/_Layout.cshtml` and `Views/Shared/_LoginPartial.cshtml`. The rendered check signs in as `author@checklists.local` (preview data). Rows with an empty Page URL need a signed-out visitor; xUnit tests check them.

| Location | Kind | Text that must appear | Page URL |
|---|---|---|---|
| Layout head | title | My Checklists - Checklists | `/` |
| Layout navbar | brand | checklist_logo.png | `/` |
| Layout navbar | brand | Checklists | `/` |
| Login partial, signed in | greeting | Hello Alex Author! | `/` |
| Login partial, signed in | link | Change Password | `/` |
| Login partial, signed in | link | Log out | `/` |
| Login partial, signed in | form | id="logoutForm" | `/` |
| Login partial, signed out | link | id="registerLink" | |
| Login partial, signed out | link | id="loginLink" | |
| Layout footer | footer | - Checklists | `/` |
| Layout styles | stylesheet | Site.css | `/` |

## Screen fields

Page URLs use the preview rows in `plan.md`: Packing list is checklist 1 and Passport is task 1.

| Screen | Old control | Kind | Text that must appear | Page URL |
|---|---|---|---|---|
| Checklists Index | heading | heading | My Checklists | `/` |
| Checklists Index | create button | link | Create new list | `/` |
| Checklists Index | list | list | id="checklists" | `/` |
| Checklists Index | checklist link | link | Packing list | `/` |
| Checklists Index | edit icon | link | title="Edit" | `/` |
| Checklists Index | delete icon | link | title="Delete" | `/` |
| Checklists Index | delete script | script | Are you sure that you want to delete the checklist? | `/` |
| Checklists Index | delete script | script | url: "api/checklists/" | `/` |
| Checklists Index | empty state | text | You don't have any checklists. | |
| Checklists Index | empty state | link | Create a checklist | |
| ChecklistForm (new) | heading | heading | New checklist | `/Checklists/New` |
| ChecklistForm | label | label | Name | `/Checklists/New` |
| ChecklistForm | input | input | name="Name" | `/Checklists/New` |
| ChecklistForm | hidden id | input | name="Id" | `/Checklists/New` |
| ChecklistForm | antiforgery | input | name="__RequestVerificationToken" | `/Checklists/New` |
| ChecklistForm | save | button | Save | `/Checklists/New` |
| ChecklistForm | cancel | link | Cancel | `/Checklists/New` |
| ChecklistForm (edit) | heading | heading | Modify checklist name | `/Checklists/Edit/1` |
| ChecklistForm (edit) | input value | input | value="Packing list" | `/Checklists/Edit/1` |
| Tasks Index | heading | heading | Packing list | `/checklists/1/tasks` |
| Tasks Index | add button | link | Add Task | `/checklists/1/tasks` |
| Tasks Index | list form | form | id="todo-items" | `/checklists/1/tasks` |
| Tasks Index | task | text | Passport | `/checklists/1/tasks` |
| Tasks Index | task | text | Charger | `/checklists/1/tasks` |
| Tasks Index | check box | input | js-check-toggle | `/checklists/1/tasks` |
| Tasks Index | delete script | script | Are you sure that you want to delete the to-do item? | `/checklists/1/tasks` |
| Tasks Index | empty state | text | There are no to-do items. | |
| Tasks Index | empty state | link | Create a new to-do item | |
| TaskForm (new) | heading | heading | New Task | `/checklists/1/tasks/new` |
| TaskForm | label | label | Title | `/checklists/1/tasks/new` |
| TaskForm | input | input | name="Title" | `/checklists/1/tasks/new` |
| TaskForm | hidden checklist | input | name="ChecklistId" | `/checklists/1/tasks/new` |
| TaskForm | save | button | Save | `/checklists/1/tasks/new` |
| TaskForm | cancel | link | Cancel | `/checklists/1/tasks/new` |
| TaskForm (edit) | heading | heading | Modify Task | `/checklists/1/tasks/edit/1` |
| TaskForm (edit) | input value | input | value="Passport" | `/checklists/1/tasks/edit/1` |
| Login | heading | heading | Log in. | `/Account/Login` |
| Login | label | label | Email | `/Account/Login` |
| Login | input | input | name="Email" | `/Account/Login` |
| Login | label | label | Password | `/Account/Login` |
| Login | input | input | name="Password" | `/Account/Login` |
| Login | label | label | Remember me? | `/Account/Login` |
| Login | input | input | name="RememberMe" | `/Account/Login` |
| Login | submit | button | value="Log in" | `/Account/Login` |
| Login | link | link | Create an account | `/Account/Login` |
| Register | heading | heading | Create an account. | `/Account/Register` |
| Register | label | label | Full name | `/Account/Register` |
| Register | input | input | name="FullName" | `/Account/Register` |
| Register | label | label | Confirm password | `/Account/Register` |
| Register | input | input | name="ConfirmPassword" | `/Account/Register` |
| Register | submit | button | value="Create account" | `/Account/Register` |
| ForgotPassword | heading | heading | Forgot your password?. | `/Account/ForgotPassword` |
| ForgotPassword | subheading | text | Enter your email. | `/Account/ForgotPassword` |
| ForgotPassword | submit | button | value="Email Link" | `/Account/ForgotPassword` |
| ForgotPasswordConfirmation | heading | heading | Forgot Password Confirmation. | `/Account/ForgotPasswordConfirmation` |
| ForgotPasswordConfirmation | text | text | Please check your email to reset your password. | `/Account/ForgotPasswordConfirmation` |
| ResetPassword | heading | heading | Reset password. | `/Account/ResetPassword?code=preview` |
| ResetPassword | subheading | text | Reset your password. | `/Account/ResetPassword?code=preview` |
| ResetPassword | hidden code | input | name="Code" | `/Account/ResetPassword?code=preview` |
| ResetPassword | submit | button | value="Reset" | `/Account/ResetPassword?code=preview` |
| ResetPasswordConfirmation | heading | heading | Reset password confirmation. | `/Account/ResetPasswordConfirmation` |
| ResetPasswordConfirmation | link | link | click here to log in | `/Account/ResetPasswordConfirmation` |
| ExternalLoginFailure | heading | heading | Login Failure. | `/Account/ExternalLoginFailure` |
| ExternalLoginFailure | text | text | Unsuccessful login with service. | `/Account/ExternalLoginFailure` |
| Error (ConfirmEmail with no code) | heading | heading | Error. | `/Account/ConfirmEmail` |
| Error | text | text | An error occurred while processing your request. | `/Account/ConfirmEmail` |
| ConfirmEmail | heading | heading | Confirm Email. | |
| ConfirmEmail | link | link | Click here to Log in | |
| SendCode | heading | heading | Send verification code | |
| SendCode | select | input | name="SelectedProvider" | |
| SendCode | submit | button | value="Submit" | |
| VerifyCode | heading | heading | Enter verification code | |
| VerifyCode | label | label | Remember this browser? | |
| VerifyCode | input | input | name="Code" | |
| ExternalLoginConfirmation | heading | heading | Association Form | |
| ExternalLoginConfirmation | submit | button | value="Register" | |
| Lockout | heading | heading | Locked out. | |
| Lockout | text | text | This account has been locked out, please try again later. | |
| Manage Index | heading | heading | My Account. | `/Manage` |
| Manage Index | subheading | text | Change your account settings | `/Manage` |
| Manage Index | label | label | Password: | `/Manage` |
| Manage Index | link | link | Change your password | `/Manage` |
| Manage Index | label | label | External Logins: | `/Manage` |
| Manage Index | link | link | Manage | `/Manage` |
| Manage Index | label | label | Two-Factor Authentication: | `/Manage` |
| Manage Index | text | text | There are no two-factor authentication providers configured. | `/Manage` |
| ChangePassword | heading | heading | Change Password | `/Manage/ChangePassword` |
| ChangePassword | label | label | Current password | `/Manage/ChangePassword` |
| ChangePassword | input | input | name="OldPassword" | `/Manage/ChangePassword` |
| ChangePassword | label | label | Confirm new password | `/Manage/ChangePassword` |
| ChangePassword | input | input | name="NewPassword" | `/Manage/ChangePassword` |
| ChangePassword | submit | button | value="Change password" | `/Manage/ChangePassword` |
| SetPassword | heading | heading | Create Password. | `/Manage/SetPassword` |
| SetPassword | subheading | text | Create Local Login | `/Manage/SetPassword` |
| SetPassword | submit | button | value="Set password" | `/Manage/SetPassword` |
| AddPhoneNumber | heading | heading | Phone Number. | `/Manage/AddPhoneNumber` |
| AddPhoneNumber | subheading | text | Add a phone number | `/Manage/AddPhoneNumber` |
| AddPhoneNumber | input | input | name="Number" | `/Manage/AddPhoneNumber` |
| VerifyPhoneNumber | heading | heading | Verify Phone Number. | `/Manage/VerifyPhoneNumber?phoneNumber=5550100` |
| VerifyPhoneNumber | subheading | text | Enter verification code | `/Manage/VerifyPhoneNumber?phoneNumber=5550100` |
| VerifyPhoneNumber | hidden phone | input | name="phoneNumber" | `/Manage/VerifyPhoneNumber?phoneNumber=5550100` |
| ManageLogins | heading | heading | Manage your external logins. | `/Manage/ManageLogins` |
| ManageLogins | text | text | There are no external authentication services configured. | `/Manage/ManageLogins` |

## Out of scope on purpose

No controller action is excluded. These rows are legacy files with no action that this run does not copy. Each is also in `plan.md` and needs approval at Gate 3.

| Old file | Action | Reason |
|---|---|---|
| `ApplicationInsights.config` | `*` | Telemetry with no instrumentation key in the repo; not an action or a screen. |
| `Properties/PublishProfiles/ChecklistsApp - Web Deploy.pubxml` | `*` | Azure deploy profile that contains a real password. Not copied. Deployment is not part of this run. |
| `Web.Debug.config` | `*` | Build transform. Development settings move to `appsettings.Development.json`. |
