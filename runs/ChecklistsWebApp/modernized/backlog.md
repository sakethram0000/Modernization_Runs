# Backlog — ChecklistsWebApp

Scope: whole application

Status: approved at Gate 4 on 2026-10-28; ready to merge. All open questions were kept as legacy behavior at Gate 3. The Phase 5 corrections below were confirmed at Gate 4. Verification: 152 of 152 tests passed and `compare-parity.ps1 -WholeApp` passed (see `test-report.md`, `walkthrough.md`).

Every scenario below describes what the legacy code does today, including the gaps listed in `plan.md` open questions. Scenario text changes only if Gate 3 approves a change.

**Corrections found while porting (Phase 5).** These lines described the legacy code wrongly. They are fixed to match the legacy code and listed again at Gate 4.

| Scenario | Was | Now | Why |
|---|---|---|---|
| US-CL-02 "Save a new checklist and the next request still shows it" | redirected to "/Checklists" | redirected to "/" | `RedirectToAction("Index")` on `ChecklistsController` matches both route defaults, so MVC 5 writes "/". |
| US-CL-03 "The author saves a new name" | redirected to "/Checklists" | redirected to "/" | Same route rule. |
| US-MG-04 "Removing a login the user does not have…" | Message "Error" | Message "RemoveLoginSuccess"; title changed from "…goes back with the error message" | Identity 2 `UserManager.RemoveLoginAsync` succeeds when the login is not there. Library behavior, like `plan.md` open question 3. |

## Summary

| Domain | Stories | Scenarios | Open questions in `plan.md` |
|---|---|---|---|
| Shared foundation | 3 | 8 | 2 |
| Accounts — sign-in and registration | 8 | 47 | 6 |
| Accounts — account settings (Manage) | 6 | 27 | 1 |
| Checklists | 5 | 24 | 3 |
| Tasks | 7 | 38 | 5 |
| Cross-domain | 5 | 7 | 0 |
| **Total** | **34** | **151** | **17** |

Preview users used in scenarios are listed in `plan.md` under Persistence. "The author" is `author@checklists.local` (Alex Author). "Another user" is `other@checklists.local` (Other User).

---

## Shared foundation

### US-SF-01 — Layout and navigation bar

**As a** visitor or signed-in user **I want to** see the same header, menu, and footer on every page **so that** the app looks and works as before.

**Legacy source:** `Views/Shared/_Layout.cshtml`, `Views/Shared/_LoginPartial.cshtml`, `Core/Extensions/IdentityExtensions.cs`, `App_Start/BundleConfig.cs`, `Content/Site.css`

```gherkin
Feature: Layout and navigation bar

  Scenario: Every page shows the brand, the page title, and the footer
    Given I open the login page
    Then the page title is "Log in - Checklists"
    And the navbar brand shows the checklist logo and the text "Checklists" linking to "/"
    And the footer shows "© <current year> - Checklists"

  Scenario: An anonymous visitor sees Register and Log in in the navbar
    Given I am not signed in
    When I open the login page
    Then the navbar shows the links "Register" and "Log in"

  Scenario: A signed-in user sees their full name, Change Password, and Log out
    Given I am signed in as the author
    When I open "/"
    Then the navbar shows "Hello Alex Author!"
    And the dropdown shows "Change Password" linking to "/Manage/ChangePassword" and "Log out"

  Scenario: Pages load the legacy stylesheets
    When I open the login page
    Then the page links bootstrap.css, bootstrap-paper.css, and site.css
    And each stylesheet returns HTTP 200 with a non-empty body
```

### US-SF-02 — Default route

**As a** user **I want** "/" to open my checklists **so that** the home page is the list.

**Legacy source:** `App_Start/RouteConfig.cs` (default `Checklists/Index`), no `HomeController`

```gherkin
Feature: Default route

  Scenario: The root URL shows the checklist list
    Given I am signed in as the author
    When I open "/"
    Then I see the heading "My Checklists"

  Scenario: The Home URL does not exist
    Given I am signed in as the author
    When I open "/Home"
    Then the response is 404
```

### US-SF-03 — Error page

**As a** user **I want** a plain error page when a flow cannot continue **so that** I know the request failed.

**Legacy source:** `Views/Shared/Error.cshtml`, `FilterConfig.cs` (`HandleErrorAttribute`), every `View("Error")` call in `AccountController` and `ManageController`

```gherkin
Feature: Error page

  Scenario: A flow that returns the Error view shows the error text
    Given I am not signed in
    When I open "/Account/ConfirmEmail" with no userId and no code
    Then I see "Error." and "An error occurred while processing your request."

  Scenario: The error page uses the layout
    When I open "/Account/ConfirmEmail" with no userId and no code
    Then the page title is "Error - Checklists"
```

---

## Accounts — sign-in and registration

### US-AC-01 — Log in

**As a** registered user **I want to** sign in with my email and password **so that** I can reach my checklists.

**Legacy source:** `AccountController.Login` (GET, POST), `RedirectToLocal`, `LoginViewModel`, `Views/Account/Login.cshtml`, `Views/Shared/Lockout.cshtml`, `Startup.Auth.cs`

```gherkin
Feature: Log in

  Scenario: The login page shows the email, password, and remember me fields
    When I open "/Account/Login"
    Then I see the heading "Log in."
    And I see the labels "Email", "Password", "Remember me?"
    And I see inputs named "Email", "Password", "RememberMe"
    And I see the button "Log in" and the link "Create an account" to "/Account/Register"

  Scenario: A correct login with a local return URL goes back to that URL
    Given the author exists with password "Preview123!"
    When I post the login form with that email and password and ReturnUrl "/"
    Then I am redirected to "/"
    And the next request to "/" shows "My Checklists"

  Scenario: A correct login with no return URL goes to /Home
    Given the author exists
    When I post the login form with no ReturnUrl
    Then I am redirected to "/Home"

  Scenario: A login with a non-local return URL goes to /Home
    Given the author exists
    When I post the login form with ReturnUrl "http://example.com/"
    Then I am redirected to "/Home"

  Scenario: A wrong password shows Invalid login attempt
    When I post the login form with the author's email and a wrong password
    Then I stay on the login form
    And I see "Invalid login attempt."
    And I am not signed in

  Scenario: An empty email and password show the required messages
    When I post the login form with an empty email and an empty password
    Then I see "The Email field is required." and "The Password field is required."

  Scenario: An email that is not an email address shows the format message
    When I post the login form with email "not-an-email"
    Then I see "The Email field is not a valid e-mail address."

  Scenario: Login without the verification token is rejected
    When I post the login form without the antiforgery token
    Then the response is 400 and I am not signed in

  Scenario: A locked-out user sees the lockout page
    Given a user whose lockout end date is in the future
    When I post the login form with that user's correct password
    Then I see "Locked out." and "This account has been locked out, please try again later."

  Scenario: Wrong passwords never lock the account
    Given the author exists
    When I post the login form with a wrong password 6 times
    And I post the login form with the correct password
    Then I am signed in

  Scenario: A user with two-factor enabled is sent to choose a code provider
    Given a user with two-factor enabled and a confirmed email
    When I post the login form with that user's correct password and ReturnUrl "/"
    Then I am redirected to "/Account/SendCode" with ReturnUrl "/" and RememberMe "False"
```

### US-AC-02 — Register

**As a** new visitor **I want to** create an account **so that** I can keep checklists.

**Legacy source:** `AccountController.Register` (GET, POST), `RegisterViewModel`, `IdentityConfig.cs` (user and password validators), `Views/Account/Register.cshtml`

```gherkin
Feature: Register

  Scenario: The register page shows the account fields
    When I open "/Account/Register"
    Then I see the heading "Create an account."
    And I see the labels "Full name", "Email", "Password", "Confirm password"
    And I see inputs named "FullName", "Email", "Password", "ConfirmPassword"
    And I see the button "Create account"

  Scenario: A valid registration creates the user, signs them in, and opens the list
    When I register "Nina New", "nina@checklists.local", password "Strong1!" confirmed
    Then I am redirected to "/"
    And the next request to "/" shows "Hello Nina New!" and "You don't have any checklists."
    And the user is stored with FullName "Nina New" and UserName "nina@checklists.local"

  Scenario: Registering an email that already exists shows the duplicate messages
    When I register with the author's email
    Then I see "Name author@checklists.local is already taken."
    And I see "Email 'author@checklists.local' is already taken."

  Scenario: A password that breaks the rules shows the password message
    When I register with password "abcdef" confirmed
    Then I see "Passwords must have at least one non letter or digit character. Passwords must have at least one digit ('0'-'9'). Passwords must have at least one uppercase ('A'-'Z')."

  Scenario: A password shorter than 6 characters shows the length message
    When I register with password "Ab1!" confirmed
    Then I see "The Password must be at least 6 characters long."

  Scenario: A registration confirmation that does not match shows the compare message
    When I register with password "Strong1!" and confirmation "Strong2!"
    Then I see "The password and confirmation password do not match."

  Scenario: A missing full name shows the required message
    When I register with an empty full name
    Then I see "The Full name field is required."

  Scenario: A full name longer than 255 characters shows the length message
    When I register with a 256-character full name
    Then I see "The field Full name must be a string with a maximum length of 255."
```

### US-AC-03 — Log out

**As a** signed-in user **I want to** log out **so that** the next person cannot use my account.

**Legacy source:** `AccountController.LogOff`, `Views/Shared/_LoginPartial.cshtml` (`logoutForm`)

```gherkin
Feature: Log out

  Scenario: Log out signs me out and returns to the list URL
    Given I am signed in as the author
    When I post "/Account/LogOff" with the antiforgery token
    Then I am redirected to "/"
    And the next request to "/" is redirected to the login page

  Scenario: Log out is POST only
    Given I am signed in as the author
    When I send GET "/Account/LogOff"
    Then the response is 404

  Scenario: Log out requires a signed-in user
    Given I am not signed in
    When I post "/Account/LogOff" with an antiforgery token
    Then I am redirected to "/Account/Login"
```

### US-AC-04 — Forgot password

**As a** user who forgot a password **I want to** ask for a reset link **so that** I can sign in again.

**Legacy source:** `AccountController.ForgotPassword` (GET, POST), `ForgotPasswordConfirmation`, `ForgotPasswordViewModel`, `IdentityConfig.EmailService` (sends nothing), `Views/Account/ForgotPassword.cshtml`, `ForgotPasswordConfirmation.cshtml`

```gherkin
Feature: Forgot password

  Scenario: The forgot password page shows the email field
    When I open "/Account/ForgotPassword"
    Then I see "Forgot your password?." and "Enter your email."
    And I see the label "Email", an input named "Email", and the button "Email Link"

  Scenario: An unknown email shows the confirmation page
    When I post the forgot password form with "nobody@checklists.local"
    Then I see "Forgot Password Confirmation." and "Please check your email to reset your password."

  Scenario: An email that is not confirmed shows the confirmation page
    Given the author's email is not confirmed
    When I post the forgot password form with the author's email
    Then I see "Please check your email to reset your password."
    And no email is sent

  Scenario: A confirmed email shows the forgot password form again
    Given a user whose email is confirmed
    When I post the forgot password form with that email
    Then I see the forgot password form again
    And no email is sent

  Scenario: An invalid email shows the format message
    When I post the forgot password form with "not-an-email"
    Then I see "The Email field is not a valid e-mail address."
```

### US-AC-05 — Reset password

**As a** user with a reset code **I want to** choose a new password **so that** I can sign in again.

**Legacy source:** `AccountController.ResetPassword` (GET, POST), `ResetPasswordConfirmation`, `ResetPasswordViewModel`, `Views/Account/ResetPassword.cshtml`, `ResetPasswordConfirmation.cshtml`

```gherkin
Feature: Reset password

  Scenario: The reset page without a code shows the error page
    When I open "/Account/ResetPassword"
    Then I see "An error occurred while processing your request."

  Scenario: The reset page with a code shows the reset form
    When I open "/Account/ResetPassword?code=abc"
    Then I see "Reset password." and "Reset your password."
    And I see the labels "Email", "Password", "Confirm password" and the button "Reset"

  Scenario: A reset for an unknown email goes to the confirmation page
    When I post the reset form for "nobody@checklists.local"
    Then I am redirected to "/Account/ResetPasswordConfirmation"

  Scenario: A reset with a bad code shows Invalid token
    When I post the reset form for the author with code "bad"
    Then I see "Invalid token."

  Scenario: A reset with a valid code changes the password
    Given a valid reset code for the author
    When I post the reset form with that code and password "Changed1!"
    Then I am redirected to "/Account/ResetPasswordConfirmation"
    And the author can sign in with "Changed1!"

  Scenario: The reset confirmation page links to the login page
    When I open "/Account/ResetPasswordConfirmation"
    Then I see "Reset password confirmation." and "Your password has been reset. Please click here to log in"
    And "click here to log in" links to "/Account/Login"
```

### US-AC-06 — Confirm email

**As a** registered user **I want to** confirm my email from a link **so that** my address is verified.

**Legacy source:** `AccountController.ConfirmEmail`, `Views/Account/ConfirmEmail.cshtml`

```gherkin
Feature: Confirm email

  Scenario: Confirm email with a missing user id or code shows the error page
    When I open "/Account/ConfirmEmail?userId=x"
    Then I see "An error occurred while processing your request."

  Scenario: Confirm email with a bad code shows the error page
    When I open "/Account/ConfirmEmail" with the author's id and code "bad"
    Then I see "An error occurred while processing your request."

  Scenario: Confirm email with a valid code confirms the email
    Given a valid email confirmation code for the author
    When I open "/Account/ConfirmEmail" with the author's id and that code
    Then I see "Confirm Email." and "Thank you for confirming your email. Please Click here to Log in"
    And the author's email is confirmed
```

### US-AC-07 — Two-factor sign-in

**As a** user with two-factor enabled **I want to** enter a code after my password **so that** my account has a second check.

**Legacy source:** `AccountController.SendCode` (GET, POST), `VerifyCode` (GET, POST), `IdentityConfig.cs` (`Phone Code`, `Email Code` providers, no-op senders), `Views/Account/SendCode.cshtml`, `VerifyCode.cshtml`

```gherkin
Feature: Two-factor sign-in

  Scenario: Send code without a password step shows the error page
    When I open "/Account/SendCode?rememberMe=false" without signing in first
    Then I see "An error occurred while processing your request."

  Scenario: Verify code without a password step shows the error page
    When I open "/Account/VerifyCode?provider=Email%20Code&rememberMe=false" without signing in first
    Then I see "An error occurred while processing your request."

  Scenario: After the password step the send code page lists the providers
    Given a user with two-factor enabled and a confirmed email has passed the password step
    When I open the send code page
    Then I see "Send." and "Send verification code"
    And the provider list contains "Email Code"
    And I see the button "Submit"

  Scenario: Choosing a provider goes to the verify code page
    Given a user has passed the password step
    When I post the send code form with provider "Email Code"
    Then I am redirected to "/Account/VerifyCode" with Provider "Email Code"

  Scenario: A wrong code shows Invalid code
    Given a user has passed the password step and chose "Email Code"
    When I post the verify code form with code "000000"
    Then I see "Invalid code."

  Scenario: A correct code signs the user in
    Given a user has passed the password step and chose "Email Code"
    When I post the verify code form with a valid "Email Code" token and ReturnUrl "/"
    Then I am redirected to "/"
    And the next request to "/" shows "My Checklists"
```

### US-AC-08 — External logins

**As a** visitor **I want** the external login screens to behave as before **so that** nothing breaks when no provider is configured.

**Legacy source:** `AccountController.ExternalLogin`, `ExternalLoginCallback`, `ExternalLoginConfirmation`, `ExternalLoginFailure`, `ChallengeResult`, `Startup.Auth.cs` (all providers commented out), `Views/Account/ExternalLoginConfirmation.cshtml`, `ExternalLoginFailure.cshtml`, `_ExternalLoginsListPartial.cshtml`

```gherkin
Feature: External logins

  Scenario: The external login failure page shows its message
    When I open "/Account/ExternalLoginFailure"
    Then I see "Login Failure." and "Unsuccessful login with service."

  Scenario: The external login callback with no external sign-in goes back to login
    When I open "/Account/ExternalLoginCallback"
    Then I am redirected to "/Account/Login"

  Scenario: External login confirmation when already signed in goes to Manage
    Given I am signed in as the author
    When I post "/Account/ExternalLoginConfirmation" with an email and the antiforgery token
    Then I am redirected to "/Manage"

  Scenario: External login confirmation with no external sign-in shows the failure page
    Given I am not signed in
    When I post "/Account/ExternalLoginConfirmation" with an email and the antiforgery token
    Then I see "Unsuccessful login with service."

  Scenario: External login with a provider that is not configured does not sign anyone in
    Given no external provider is configured
    When I post "/Account/ExternalLogin" with provider "Google" and the antiforgery token
    Then I am not signed in
```

---

## Accounts — account settings (Manage)

### US-MG-01 — Account settings page

**As a** signed-in user **I want to** see my account settings **so that** I can reach password and login options.

**Legacy source:** `ManageController.Index`, `ManageMessageId`, `IndexViewModel`, `Views/Manage/Index.cshtml`

```gherkin
Feature: Account settings page

  Scenario: The settings page shows password, external logins, and two-factor sections
    Given I am signed in as the author
    When I open "/Manage"
    Then I see "My Account." and "Change your account settings"
    And I see "Password:" with the link "Change your password" to "/Manage/ChangePassword"
    And I see "External Logins:" with "0" and the link "Manage" to "/Manage/ManageLogins"
    And I see "Two-Factor Authentication:" and "There are no two-factor authentication providers configured."

  Scenario: A status message is shown for each message id
    Given I am signed in as the author
    When I open "/Manage?message=AddPhoneSuccess"
    Then I see "Your phone number was added."
    And "/Manage?message=RemovePhoneSuccess" shows "Your phone number was removed."
    And "/Manage?message=Error" shows "An error has occurred."
    And "/Manage?message=ChangePasswordSuccess" shows "Your password has been changed."
    And "/Manage?message=SetPasswordSuccess" shows "Your password has been set."
    And "/Manage?message=SetTwoFactorSuccess" shows "Your two-factor authentication provider has been set."

  Scenario: The settings page requires a signed-in user
    Given I am not signed in
    When I open "/Manage"
    Then I am redirected to "/Account/Login" with ReturnUrl "/Manage"
```

### US-MG-02 — Change password

**As a** signed-in user **I want to** change my password **so that** I can keep my account safe.

**Legacy source:** `ManageController.ChangePassword` (GET, POST), `ChangePasswordViewModel`, `Views/Manage/ChangePassword.cshtml`

```gherkin
Feature: Change password

  Scenario: The change password page shows the three password fields
    Given I am signed in as the author
    When I open "/Manage/ChangePassword"
    Then I see the heading "Change Password"
    And I see the labels "Current password", "New password", "Confirm new password"
    And I see inputs named "OldPassword", "NewPassword", "ConfirmPassword" and the button "Change password"

  Scenario: A correct change saves the new password and returns to the list
    Given I am signed in as the author
    When I post the change password form with current "Preview123!" and new "Changed1!" confirmed
    Then I am redirected to "/"
    And the author can sign in with "Changed1!"

  Scenario: A wrong current password shows Incorrect password
    Given I am signed in as the author
    When I post the change password form with current "wrong" and new "Changed1!" confirmed
    Then I see "Incorrect password."

  Scenario: A change password confirmation that does not match shows the compare message
    Given I am signed in as the author
    When I post the change password form with new "Changed1!" and confirmation "Changed2!"
    Then I see "The new password and confirmation password do not match."

  Scenario: A new password shorter than 6 characters shows the length message
    Given I am signed in as the author
    When I post the change password form with new "Ab1!" confirmed
    Then I see "The New password must be at least 6 characters long."
```

### US-MG-03 — Set password

**As a** user who signed up through an external login **I want to** add a local password **so that** I can sign in without the provider.

**Legacy source:** `ManageController.SetPassword` (GET, POST), `SetPasswordViewModel`, `Views/Manage/SetPassword.cshtml`

```gherkin
Feature: Set password

  Scenario: The set password page shows the local login form
    Given I am signed in as the author
    When I open "/Manage/SetPassword"
    Then I see "Create Password." and "Create Local Login"
    And I see the labels "New password", "Confirm new password" and the button "Set password"

  Scenario: A user who already has a password cannot set another one
    Given I am signed in as the author
    When I post the set password form with "Another1!" confirmed
    Then I see "User already has a password set."

  Scenario: A set password confirmation that does not match shows the compare message
    Given I am signed in as the author
    When I post the set password form with "Another1!" and confirmation "Another2!"
    Then I see "The new password and confirmation password do not match."
```

### US-MG-04 — Manage external logins

**As a** signed-in user **I want to** see and manage external logins **so that** I can link or remove providers.

**Legacy source:** `ManageController.ManageLogins`, `RemoveLogin`, `LinkLogin`, `LinkLoginCallback`, `Views/Manage/ManageLogins.cshtml`

```gherkin
Feature: Manage external logins

  Scenario: With no provider configured the page says so
    Given I am signed in as the author
    When I open "/Manage/ManageLogins"
    Then I see "Manage your external logins." and "There are no external authentication services configured."

  Scenario: The error message id shows the error text
    Given I am signed in as the author
    When I open "/Manage/ManageLogins?message=Error"
    Then I see "An error has occurred."

  Scenario: Removing a login the user does not have goes back to the logins page
    Given I am signed in as the author
    When I post "/Manage/RemoveLogin" with provider "Google" and key "x" and the antiforgery token
    Then I am redirected to "/Manage/ManageLogins" with Message "RemoveLoginSuccess"

  Scenario: The link login callback with no external sign-in goes back with the error message
    Given I am signed in as the author
    When I open "/Manage/LinkLoginCallback"
    Then I am redirected to "/Manage/ManageLogins" with Message "Error"

  Scenario: Linking a provider that is not configured does not add a login
    Given I am signed in as the author
    When I post "/Manage/LinkLogin" with provider "Google" and the antiforgery token
    Then the author still has 0 external logins
```

### US-MG-05 — Phone number

**As a** signed-in user **I want to** add, verify, and remove a phone number **so that** it can be used for codes.

**Legacy source:** `ManageController.AddPhoneNumber` (GET, POST), `VerifyPhoneNumber` (GET, POST), `RemovePhoneNumber`, `AddPhoneNumberViewModel`, `VerifyPhoneNumberViewModel`, `IdentityConfig.SmsService` (sends nothing), `Views/Manage/AddPhoneNumber.cshtml`, `VerifyPhoneNumber.cshtml`

```gherkin
Feature: Phone number

  Scenario: The add phone page shows the number field
    Given I am signed in as the author
    When I open "/Manage/AddPhoneNumber"
    Then I see "Phone Number." and "Add a phone number"
    And I see the label "Phone Number", an input named "Number", and the button "Submit"

  Scenario: A valid number goes to the verify page without sending a text
    Given I am signed in as the author
    When I post the add phone form with "5551234567"
    Then I am redirected to "/Manage/VerifyPhoneNumber" with PhoneNumber "5551234567"
    And no text message is sent

  Scenario: An invalid number shows the phone message
    Given I am signed in as the author
    When I post the add phone form with "abc"
    Then I see "The Phone Number field is not a valid phone number."

  Scenario: The verify page without a number shows the error page
    Given I am signed in as the author
    When I open "/Manage/VerifyPhoneNumber"
    Then I see "An error occurred while processing your request."

  Scenario: The verify page with a number shows the code form
    Given I am signed in as the author
    When I open "/Manage/VerifyPhoneNumber?phoneNumber=5551234567"
    Then I see "Verify Phone Number." and "Enter verification code"
    And I see the label "Code" and the button "Submit"

  Scenario: A wrong code shows Failed to verify phone
    Given I am signed in as the author
    When I post the verify phone form with "5551234567" and code "000000"
    Then I see "Failed to verify phone"

  Scenario: A correct code saves the number
    Given I am signed in as the author and have a valid phone code for "5551234567"
    When I post the verify phone form with that code
    Then I am redirected to "/Manage" with Message "AddPhoneSuccess"
    And the author's phone number is "5551234567"

  Scenario: Remove phone number clears the number
    Given I am signed in as the author with phone number "5551234567"
    When I post "/Manage/RemovePhoneNumber" with the antiforgery token
    Then I am redirected to "/Manage" with Message "RemovePhoneSuccess"
    And the author has no phone number
```

### US-MG-06 — Turn two-factor on and off

**As a** signed-in user **I want to** enable or disable two-factor sign-in **so that** I control the second check.

**Legacy source:** `ManageController.EnableTwoFactorAuthentication`, `DisableTwoFactorAuthentication`

```gherkin
Feature: Turn two-factor on and off

  Scenario: Enable two-factor turns it on and returns to Manage
    Given I am signed in as the author
    When I post "/Manage/EnableTwoFactorAuthentication" with the antiforgery token
    Then I am redirected to "/Manage"
    And two-factor is enabled for the author

  Scenario: Disable two-factor turns it off and returns to Manage
    Given I am signed in as the author with two-factor enabled
    When I post "/Manage/DisableTwoFactorAuthentication" with the antiforgery token
    Then I am redirected to "/Manage"
    And two-factor is disabled for the author

  Scenario: Enable two-factor without the verification token is rejected
    Given I am signed in as the author
    When I post "/Manage/EnableTwoFactorAuthentication" without the antiforgery token
    Then the response is 400
```

---

## Checklists

### US-CL-01 — See my checklists

**As a** signed-in author **I want to** see the checklists I created **so that** I can open one.

**Legacy source:** `ChecklistsController.Index`, `ChecklistRepository.GetChecklistsCreatedByUser`, `Views/Checklists/Index.cshtml`

```gherkin
Feature: See my checklists

  Scenario: Signed-in user sees only their own checklists
    Given I am signed in as the author
    When I open "/"
    Then I see "My Checklists" and the button "Create new list" linking to "/Checklists/New"
    And I see "Packing list"
    And I do not see "Other list"

  Scenario: A soft-deleted checklist is hidden from the list
    Given I am signed in as the author
    When I open "/"
    Then I do not see "Old list"

  Scenario: Each checklist has edit and delete controls
    Given I am signed in as the author
    When I open "/"
    Then "Packing list" has an Edit link to "/Checklists/Edit/1" and a Delete control with data-checklist-id "1"

  Scenario: A user with no checklists sees the empty message
    Given I am signed in as "empty@checklists.local"
    When I open "/"
    Then I see "You don't have any checklists." and the link "Create a checklist" to "/Checklists/New"

  Scenario: An anonymous visit to the checklist list is sent to login
    Given I am not signed in
    When I open "/"
    Then I am redirected to "/Account/Login" with ReturnUrl "/"
```

### US-CL-02 — Create a checklist

**As a** signed-in user **I want to** create a checklist **so that** I can add tasks to it.

**Legacy source:** `ChecklistsController.New`, `Save`, `ChecklistFormViewModel`, `Checklist` constructor, `Views/Checklists/ChecklistForm.cshtml`

```gherkin
Feature: Create a checklist

  Scenario: New checklist form shows an empty name
    Given I am signed in as the author
    When I open "/Checklists/New"
    Then I see the heading "New checklist", the label "Name", an empty input named "Name", a hidden "Id" of 0
    And I see the button "Save" and the link "Cancel"

  Scenario: Save a new checklist and the next request still shows it
    Given I am signed in as the author
    When I post the checklist form with Id 0 and Name "Groceries"
    Then I am redirected to "/"
    And a later request to "/" shows "Groceries"
    And the stored checklist has AuthorId of the author, IsDeleted false, and a DateAdded

  Scenario: Save with an empty name shows the required message
    Given I am signed in as the author
    When I post the checklist form with an empty name
    Then I see "The Name field is required." and no checklist is created

  Scenario: Save with a name longer than 255 characters shows the length message
    Given I am signed in as the author
    When I post the checklist form with a 256-character name
    Then I see "The field Name must be a string with a maximum length of 255."

  Scenario: Save without the verification token is rejected
    Given I am signed in as the author
    When I post the checklist form without the antiforgery token
    Then the response is 400 and no checklist is created
```

### US-CL-03 — Rename a checklist

**As a** checklist author **I want to** rename a checklist **so that** the name stays accurate.

**Legacy source:** `ChecklistsController.Edit`, `Save`, `ChecklistRepository.GetChecklist` (no `IsDeleted` filter), `ChecklistFormViewModel.Title`

```gherkin
Feature: Rename a checklist

  Scenario: The author opens the rename form
    Given I am signed in as the author
    When I open "/Checklists/Edit/1"
    Then I see the heading "Modify checklist name" and the Name input contains "Packing list"

  Scenario: Edit without an id is not found
    Given I am signed in as the author
    When I open "/Checklists/Edit"
    Then the response is 404

  Scenario: Edit of an unknown id is not found
    Given I am signed in as the author
    When I open "/Checklists/Edit/9999"
    Then the response is 404

  Scenario: A different signed-in user cannot open the rename form
    Given I am signed in as another user
    When I open "/Checklists/Edit/1"
    Then I am redirected to "/Account/Login"

  Scenario: The author saves a new name
    Given I am signed in as the author
    When I post the checklist form with Id 1 and Name "Holiday packing"
    Then I am redirected to "/" and the list shows "Holiday packing"

  Scenario: Save of an unknown id is not found and does not create a checklist
    Given I am signed in as the author
    When I post the checklist form with Id 9999 and Name "Ghost"
    Then the response is 404 and no checklist named "Ghost" exists

  Scenario: Save of a soft-deleted checklist changes the name and leaves it deleted
    Given I am signed in as the author
    When I post the checklist form with the id of "Old list" and Name "Renamed old"
    Then the stored checklist is named "Renamed old" and is still deleted
    And it is not on the list

  Scenario: A different signed-in user can change the name through Save
    Given I am signed in as another user
    When I post the checklist form with Id 1 and Name "Changed by other"
    Then checklist 1 is named "Changed by other"
```

### US-CL-04 — Delete a checklist

**As a** checklist author **I want to** delete a checklist **so that** it leaves my list.

**Legacy source:** API `Apis/ChecklistsController.Delete`, `Checklist.Delete`, `Views/Checklists/Index.cshtml` (bootbox confirm and `DELETE api/checklists/{id}`)

```gherkin
Feature: Delete a checklist

  Scenario: The author deletes a checklist and it leaves the list
    Given I am signed in as the author
    When I send DELETE "/api/checklists/1"
    Then the response is 204 with an empty body
    And a later request to "/" does not show "Packing list"
    And the stored checklist 1 has IsDeleted true

  Scenario: Delete of a missing checklist is not found
    Given I am signed in as the author
    When I send DELETE "/api/checklists/9999"
    Then the response is 404

  Scenario: Delete of an already deleted checklist is not found
    Given I am signed in as the author
    When I send DELETE "/api/checklists/<id of Old list>"
    Then the response is 404

  Scenario: A different signed-in user cannot delete the checklist
    Given I am signed in as another user
    When I send DELETE "/api/checklists/1"
    Then the response is 401 and checklist 1 is not deleted

  Scenario: An anonymous delete is rejected
    Given I am not signed in
    When I send DELETE "/api/checklists/1" as an AJAX request
    Then the response is 401
```

### US-CL-05 — Delete confirmation on the list

**As a** checklist author **I want** a confirmation before deleting **so that** I do not delete by mistake.

**Legacy source:** `Views/Checklists/Index.cshtml` script

```gherkin
Feature: Delete confirmation on the list

  Scenario: The list page carries the legacy delete script
    Given I am signed in as the author
    When I open "/"
    Then the page contains "Are you sure that you want to delete the checklist?"
    And the script sends DELETE to "api/checklists/" plus the checklist id
```

---

## Tasks

### US-TK-01 — See the tasks in a checklist

**As a** checklist author **I want to** see the tasks in a checklist **so that** I can work through them.

**Legacy source:** `TasksController.Index` (route `checklists/{checklistId}/tasks`), `TaskRepository.GetTasksFromChecklist`, `TaskListingViewModel`, `Views/Tasks/Index.cshtml`

```gherkin
Feature: See the tasks in a checklist

  Scenario: The author sees the checklist name and its tasks
    Given I am signed in as the author
    When I open "/checklists/1/tasks"
    Then the page title is "Packing list - Checklists" and the heading shows "Packing list"
    And I see the button "Add Task" linking to "/checklists/1/tasks/new"
    And I see "Passport" and "Charger"

  Scenario: A soft-deleted task is hidden
    Given I am signed in as the author
    When I open "/checklists/1/tasks"
    Then I do not see "Old task"

  Scenario: A checked task shows a checked box
    Given I am signed in as the author
    When I open "/checklists/1/tasks"
    Then the checkbox for "Charger" is checked and the checkbox for "Passport" is not

  Scenario: Each task has edit and delete controls
    Given I am signed in as the author
    When I open "/checklists/1/tasks"
    Then "Passport" has an Edit link to "/checklists/1/tasks/edit/<id of Passport>" and a delete control with its data-item-id

  Scenario: A checklist with no tasks shows the empty message
    Given I am signed in as the author and own a checklist with no tasks
    When I open its task list
    Then I see "There are no to-do items." and the link "Create a new to-do item"

  Scenario: An unknown checklist is not found
    Given I am signed in as the author
    When I open "/checklists/9999/tasks"
    Then the response is 404

  Scenario: Another user is sent to login
    Given I am signed in as another user
    When I open "/checklists/1/tasks"
    Then I am redirected to "/Account/Login"

  Scenario: An anonymous visit is sent to login
    Given I am not signed in
    When I open "/checklists/1/tasks"
    Then I am redirected to "/Account/Login"
```

### US-TK-02 — Add a task

**As a** checklist author **I want to** add a task **so that** the list is complete.

**Legacy source:** `TasksController.New` (route `checklists/{checklistId}/tasks/new`), `Save`, `TaskFormViewModel`, `Views/Tasks/TaskForm.cshtml`

```gherkin
Feature: Add a task

  Scenario: The new task form shows an empty title for this checklist
    Given I am signed in as the author
    When I open "/checklists/1/tasks/new"
    Then I see the heading "New Task", the label "Title", an empty input named "Title"
    And hidden inputs "Id" 0 and "ChecklistId" 1, the button "Save", and the link "Cancel"

  Scenario: New task for an unknown checklist is not found
    Given I am signed in as the author
    When I open "/checklists/9999/tasks/new"
    Then the response is 404

  Scenario: Another user cannot open the new task form
    Given I am signed in as another user
    When I open "/checklists/1/tasks/new"
    Then I am redirected to "/Account/Login"

  Scenario: Save a new task and the task list shows it
    Given I am signed in as the author
    When I post the task form with Id 0, ChecklistId 1, Title "Sunscreen"
    Then I am redirected to "/checklists/1/tasks"
    And a later request to "/checklists/1/tasks" shows "Sunscreen" unchecked

  Scenario: Save with an empty title shows the required message
    Given I am signed in as the author
    When I post the task form with an empty title
    Then I see "The Title field is required." and no task is created

  Scenario: Save with a title longer than 255 characters shows the length message
    Given I am signed in as the author
    When I post the task form with a 256-character title
    Then I see "The field Title must be a string with a maximum length of 255."

  Scenario: Save task without the verification token is rejected
    Given I am signed in as the author
    When I post the task form without the antiforgery token
    Then the response is 400 and no task is created
```

### US-TK-03 — Rename a task

**As a** checklist author **I want to** rename a task **so that** it says the right thing.

**Legacy source:** `TasksController.Edit` (route `checklists/{checklistId}/tasks/edit/{id}`), `Save`, `TaskRepository.GetTaskFromChecklist`

```gherkin
Feature: Rename a task

  Scenario: The author opens the task rename form
    Given I am signed in as the author
    When I open "/checklists/1/tasks/edit/<id of Passport>"
    Then I see the heading "Modify Task" and the Title input contains "Passport"

  Scenario: Edit of a task in a different checklist id is not found
    Given I am signed in as the author
    When I open "/checklists/<other checklist id>/tasks/edit/<id of Passport>"
    Then the response is 404

  Scenario: Edit of a soft-deleted task is not found
    Given I am signed in as the author
    When I open "/checklists/1/tasks/edit/<id of Old task>"
    Then the response is 404

  Scenario: Another user cannot open the task rename form
    Given I am signed in as another user
    When I open "/checklists/1/tasks/edit/<id of Passport>"
    Then I am redirected to "/Account/Login"

  Scenario: The author saves a new task title
    Given I am signed in as the author
    When I post the task form with the id of "Passport", ChecklistId 1, Title "Passport and visa"
    Then I am redirected to "/checklists/1/tasks" and the list shows "Passport and visa"

  Scenario: Save of an unknown task is not found
    Given I am signed in as the author
    When I post the task form with Id 9999, ChecklistId 1, Title "Ghost"
    Then the response is 404
```

### US-TK-04 — Task save has no owner check

**As a** reviewer **I want** the legacy behavior of task save written down **so that** Gate 3 can decide whether to keep it.

**Legacy source:** `TasksController` (no `[Authorize]`), `TasksController.Save(Task task)` (binds the entity, no author check)

```gherkin
Feature: Task save has no owner check

  Scenario: Another signed-in user can add a task to someone else's checklist
    Given I am signed in as another user
    When I post the task form with Id 0, ChecklistId 1, Title "Added by other"
    Then I am redirected to "/checklists/1/tasks"
    And checklist 1 has a task "Added by other"

  Scenario: An anonymous visitor can add a task through Save
    Given I am not signed in
    When I post the task form with Id 0, ChecklistId 1, Title "Added anonymously" and a valid antiforgery token
    Then checklist 1 has a task "Added anonymously"
```

### US-TK-05 — Delete a task

**As a** checklist author **I want to** delete a task **so that** it leaves the list.

**Legacy source:** API `Apis/TasksController.Delete`, `TaskRepository.GetTaskWithChecklist` (no `IsDeleted` filter), `Task.Delete`, `Views/Tasks/Index.cshtml` script

```gherkin
Feature: Delete a task

  Scenario: The author deletes a task and it leaves the list
    Given I am signed in as the author
    When I send DELETE "/api/tasks/<id of Passport>"
    Then the response is 204 with an empty body
    And a later request to "/checklists/1/tasks" does not show "Passport"

  Scenario: Delete of a missing task is not found
    Given I am signed in as the author
    When I send DELETE "/api/tasks/9999"
    Then the response is 404

  Scenario: Delete of an already deleted task succeeds again
    Given I am signed in as the author
    When I send DELETE "/api/tasks/<id of Old task>"
    Then the response is 204

  Scenario: Another user cannot delete the task
    Given I am signed in as another user
    When I send DELETE "/api/tasks/<id of Passport>"
    Then the response is 401 and the task is not deleted

  Scenario: An anonymous task delete is rejected
    Given I am not signed in
    When I send DELETE "/api/tasks/<id of Passport>" as an AJAX request
    Then the response is 401

  Scenario: The task list carries the legacy delete and check scripts
    Given I am signed in as the author
    When I open "/checklists/1/tasks"
    Then the page contains "Are you sure that you want to delete the to-do item?"
    And the script calls "/api/tasks/" with "/check" for check and uncheck
```

### US-TK-06 — Check a task

**As a** checklist author **I want to** tick a task **so that** I can see what is done.

**Legacy source:** API `Apis/TasksController.Check` (`PUT api/tasks/{id}/check`), `Task.Check`

```gherkin
Feature: Check a task

  Scenario: Checking a task marks it checked
    Given I am signed in as the author
    When I send PUT "/api/tasks/<id of Passport>/check"
    Then the response is 200 with an empty body
    And the stored task is checked

  Scenario: Checking a task that is already checked keeps it checked
    Given I am signed in as the author
    When I send PUT "/api/tasks/<id of Charger>/check"
    Then the response is 200 and the task is still checked

  Scenario: Checking a missing task is not found
    Given I am signed in as the author
    When I send PUT "/api/tasks/9999/check"
    Then the response is 404

  Scenario: Another user cannot check the task
    Given I am signed in as another user
    When I send PUT "/api/tasks/<id of Passport>/check"
    Then the response is 401

  Scenario: Checking a deleted task is a bad request
    Given I am signed in as the author
    When I send PUT "/api/tasks/<id of Old task>/check"
    Then the response is 400 with JSON {"Message":"Cannot check a deleted item"}
```

### US-TK-07 — Uncheck a task

**As a** checklist author **I want to** untick a task **so that** I can redo it.

**Legacy source:** API `Apis/TasksController.Uncheck` (`DELETE api/tasks/{id}/check`), `Task.Uncheck`

```gherkin
Feature: Uncheck a task

  Scenario: Unchecking a task clears the check
    Given I am signed in as the author
    When I send DELETE "/api/tasks/<id of Charger>/check"
    Then the response is 204 and the stored task is not checked

  Scenario: Unchecking a missing task is not found
    Given I am signed in as the author
    When I send DELETE "/api/tasks/9999/check"
    Then the response is 404

  Scenario: Another user cannot uncheck the task
    Given I am signed in as another user
    When I send DELETE "/api/tasks/<id of Charger>/check"
    Then the response is 401

  Scenario: Unchecking a deleted task is a bad request
    Given I am signed in as the author
    When I send DELETE "/api/tasks/<id of Old task>/check"
    Then the response is 400 with JSON {"Message":"Cannot uncheck a deleted item"}
```

---

## Cross-domain

One story per dependency in `knowledge-graph.md`. Each scenario goes through both domains in one test.

### US-XD-01 — A new account owns the checklists it creates (Accounts → Checklists)

**Legacy source:** `AccountController.Register`, `ChecklistsController.Save` (`AuthorId = User.Identity.GetUserId()`), `GetChecklistsCreatedByUser`

```gherkin
Feature: A new account owns the checklists it creates

  Scenario: Register, create a checklist, and see it on the list
    When I register "Nina New" with "nina@checklists.local"
    And I create the checklist "Nina's list"
    Then "/" shows "Nina's list"
    And the stored checklist's AuthorId is Nina's user id

  Scenario: A checklist created by one account is not on another account's list
    Given Nina created "Nina's list"
    When I sign in as the author and open "/"
    Then I do not see "Nina's list"
```

### US-XD-02 — Tasks belong to a checklist (Checklists → Tasks)

**Legacy source:** `Views/Checklists/Index.cshtml` (link to `Tasks.Index`), `TasksController.Index`, `TasksController.Save`

```gherkin
Feature: Tasks belong to a checklist

  Scenario: Create a checklist, follow its link, and add a task
    Given I am signed in as the author
    When I create the checklist "Weekend"
    And I follow the "Weekend" link on "/"
    Then I am on "/checklists/<new id>/tasks" with the heading "Weekend"
    When I add the task "Buy bread"
    Then the "Weekend" task list shows "Buy bread"

  Scenario: Renaming a checklist changes the task page heading
    Given I am signed in as the author
    When I rename checklist 1 to "Trip"
    Then "/checklists/1/tasks" shows the heading "Trip"
```

### US-XD-03 — A deleted checklist keeps its task page reachable (Checklists → Tasks)

**Legacy source:** `Apis/ChecklistsController.Delete`, `ChecklistRepository.GetChecklist` (no `IsDeleted` filter), `TasksController.Index`

```gherkin
Feature: A deleted checklist keeps its task page reachable

  Scenario: Delete a checklist and its task page still opens by URL
    Given I am signed in as the author
    When I send DELETE "/api/checklists/1"
    Then "/" does not show "Packing list"
    And "/checklists/1/tasks" still shows "Passport"
```

### US-XD-04 — Task access follows the checklist author (Tasks → Checklists → Accounts)

**Legacy source:** `TasksController.Index`, `New`, `Edit`, `Apis/TasksController` (author check through `task.Checklist.AuthorId`)

```gherkin
Feature: Task access follows the checklist author

  Scenario: A user who creates a checklist can work its tasks and another user cannot
    Given Nina registered and created the checklist "Nina's list" with the task "Nina's task"
    When the author signs in and sends PUT "/api/tasks/<id of Nina's task>/check"
    Then the response is 401
    When Nina signs in and sends PUT "/api/tasks/<id of Nina's task>/check"
    Then the response is 200
```

### US-XD-05 — The layout shows the signed-in account (Shared → Accounts)

**Legacy source:** `_LoginPartial.cshtml`, `ApplicationUser.GenerateUserIdentityAsync` (`FullName` claim), `ManageController.ChangePassword`

```gherkin
Feature: The layout shows the signed-in account

  Scenario: After changing the password the navbar still shows the full name
    Given I am signed in as the author
    When I change my password to "Changed1!"
    Then "/" shows "Hello Alex Author!"
```
