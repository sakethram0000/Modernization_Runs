# Backlog — Checklists

Status: ready to merge. Gate 4 approved. The Checklists module is complete for this run.

Scope from Gate 2: MVC `ChecklistsController` and API `ChecklistsController.Delete`. UI is ASP.NET Core MVC. Tasks, Account, and Manage stay out of the module except the login page this slice needs for the existing `[Authorize]` redirect.

`GetChecklistsCreatedByUser` keeps a row when `AuthorId` matches the signed-in user and `IsDeleted` is false. That closes discovery question 3.

## US-001 — See my checklists

**As a** signed-in author
**I want to** see the checklists I created
**So that** I can open one

**Legacy source:** `ChecklistsController.Index`, `ChecklistRepository.GetChecklistsCreatedByUser`, `Views/Checklists/Index.cshtml`

**Notes:** The name links to `Tasks/Index`. Tasks are out of scope, so that destination is not built. The visible name stays.

### Feature: See my checklists

```gherkin
Scenario: Signed-in user sees only their own checklists
  Given Alex Author is signed in
  And Alex has a checklist named "Packing list"
  And Other User has a checklist named "Other list"
  When Alex opens the checklist list
  Then the page shows "My Checklists"
  And the page shows "Packing list"
  And the page shows "Create new list"
  And the page does not show "Other list"

Scenario: A soft-deleted checklist is hidden from the list
  Given Alex Author is signed in
  And Alex has a deleted checklist named "Old list"
  When Alex opens the checklist list
  Then the page does not show "Old list"

Scenario: A user with no checklists sees the empty message
  Given Empty User is signed in
  And Empty User has no checklists
  When Empty User opens the checklist list
  Then the page shows "You don't have any checklists."
  And the page shows "Create a checklist"
```

## US-002 — Create a checklist

**As a** signed-in author
**I want to** name a new checklist and save it
**So that** it appears in my list

**Legacy source:** `ChecklistsController.New`, `ChecklistsController.Save`, `ChecklistFormViewModel`, `Checklist` constructor, `Views/Checklists/ChecklistForm.cshtml`

**Notes:** `Id == 0` creates the row. `AuthorId` is the signed-in user id. `DateAdded` is set in the constructor. `IsDeleted` stays false. The form heading and the page title are `New checklist`.

### Feature: Create a checklist

```gherkin
Scenario: New checklist form shows an empty name
  Given Alex Author is signed in
  When Alex opens the new checklist form
  Then the page shows "New checklist"
  And the page shows "Name"
  And the page shows "Save"
  And the page shows "Cancel"

Scenario: Save a new checklist and the next request still shows it
  Given Alex Author is signed in
  When Alex saves a new checklist named "Airport"
  Then the response redirects to the checklist list
  When Alex opens the checklist list again
  Then the page shows "Airport"

Scenario: Save with an empty name shows the required message
  Given Alex Author is signed in
  When Alex saves a checklist with an empty name
  Then the response is the checklist form
  And the page shows "The Name field is required."

Scenario: Save with a name longer than 255 characters shows the length message
  Given Alex Author is signed in
  When Alex saves a checklist whose name is 256 characters
  Then the response is the checklist form
  And the page shows "The field Name must be a string with a maximum length of 255."

Scenario: Save without the verification token is rejected
  Given Alex Author is signed in
  When Alex posts a new checklist named "Airport" without the antiforgery token
  Then the checklist is not saved
```

## US-003 — Rename a checklist

**As a** signed-in author
**I want to** change my checklist name
**So that** the list shows the new name

**Legacy source:** `ChecklistsController.Edit`, `ChecklistsController.Save`, `ChecklistRepository.GetChecklist`, `ChecklistFormViewModel.Title`

**Notes:** Edit returns not found when the id is missing or the row does not exist. Edit returns unauthorized when `AuthorId` is not the signed-in user. Save of an existing id copies `Name` only. Save does not compare `AuthorId`. GetChecklist does not filter `IsDeleted`, so a deleted row can still be renamed and stays deleted.

### Feature: Rename a checklist

```gherkin
Scenario: The author opens the rename form
  Given Alex Author is signed in
  And Alex has a checklist named "Packing list"
  When Alex opens that checklist for edit
  Then the page shows "Modify checklist name"
  And the page shows "Packing list"

Scenario: Edit without an id is not found
  Given Alex Author is signed in
  When Alex opens the edit action without an id
  Then the response is not found

Scenario: Edit of an unknown id is not found
  Given Alex Author is signed in
  When Alex opens edit for an id that does not exist
  Then the response is not found

Scenario: A different signed-in user cannot open the rename form
  Given Other User is signed in
  And Alex has a checklist named "Packing list"
  When Other User opens that checklist for edit
  Then the response is unauthorized

Scenario: The author saves a new name
  Given Alex Author is signed in
  And Alex has a checklist named "Packing list"
  When Alex saves that checklist with the name "Travel list"
  Then the next checklist list shows "Travel list"
  And the next checklist list does not show "Packing list"

Scenario: Save of an unknown id is not found and does not create a checklist
  Given Alex Author is signed in
  When Alex saves a checklist with an id that does not exist and the name "Ghost"
  Then the response is not found
  And the checklist list does not show "Ghost"

Scenario: Save of a soft-deleted checklist changes the name and leaves it deleted
  Given Alex Author is signed in
  And Alex has a deleted checklist named "Old list"
  When Alex saves that checklist with the name "Older list"
  Then the checklist list does not show "Older list"
  And a later delete of that id is not found

Scenario: A different signed-in user can change the name through Save
  Given Other User is signed in
  And Alex has a checklist named "Packing list"
  When Other User saves that checklist with the name "Changed by other"
  Then the response redirects to the checklist list
  And Alex's next checklist list shows "Changed by other"
```

## US-004 — Delete a checklist

**As a** signed-in author
**I want to** delete one of my checklists
**So that** it disappears from my list

**Legacy source:** `Apis/ChecklistsController.Delete`, `Checklist.Delete`, `Views/Checklists/Index.cshtml` script

**Notes:** Delete sets `IsDeleted` and returns 204. A missing row or a row that is already deleted returns 404. A different author returns 401. The list page contains the confirm text `Are you sure that you want to delete the checklist?` plus the button labels `Yes` and `No`.

### Feature: Delete a checklist

```gherkin
Scenario: The author deletes a checklist and it leaves the list
  Given Alex Author is signed in
  And Alex has a checklist named "Packing list"
  When Alex deletes that checklist
  Then the response is no content
  And Alex's next checklist list does not show "Packing list"

Scenario: Delete of a missing checklist is not found
  Given Alex Author is signed in
  When Alex deletes an id that does not exist
  Then the response is not found

Scenario: Delete of an already deleted checklist is not found
  Given Alex Author is signed in
  And Alex has a deleted checklist named "Old list"
  When Alex deletes that checklist
  Then the response is not found

Scenario: A different signed-in user cannot delete the checklist
  Given Other User is signed in
  And Alex has a checklist named "Packing list"
  When Other User deletes that checklist
  Then the response is unauthorized
  And Alex's next checklist list shows "Packing list"

Scenario: An anonymous delete is rejected
  Given nobody is signed in
  And Alex has a checklist named "Packing list"
  When that checklist is deleted without signing in
  Then the response is unauthorized
  And a later signed-in list for Alex shows "Packing list"
```

## US-005 — Sign-in gate

**As a** visitor
**I want to** be sent to login when I open a checklist page
**So that** checklist pages stay behind the existing cookie sign-in

**Legacy source:** `[Authorize]` on both checklist controllers, `Startup.Auth` login path `/Account/Login`, `Views/Shared/_Layout.cshtml`, `Views/Shared/_LoginPartial.cshtml`, `Views/Account/Login.cshtml`

**Notes:** This run does not modernize register, password reset, external login, or Manage. The login page is included because checklist pages redirect there. The signed-in header uses the `FullName` claim: `Hello Alex Author!`

### Feature: Sign-in gate

```gherkin
Scenario: An anonymous visit to the checklist list is sent to login
  Given nobody is signed in
  When a visitor opens the checklist list
  Then the response redirects to a URL containing "/Account/Login"

Scenario: The login page shows the email and password fields
  Given nobody is signed in
  When a visitor opens the login page
  Then the page shows "Log in"
  And the page shows "Email"
  And the page shows "Password"
  And the page shows "Remember me?"
  And the page shows "Register"

Scenario: A signed-in checklist page shows the author header
  Given Alex Author is signed in
  When Alex opens the checklist list
  Then the page shows "Hello Alex Author!"
  And the page shows "Change Password"
  And the page shows "Log out"
  And the page shows "Checklists"
```
