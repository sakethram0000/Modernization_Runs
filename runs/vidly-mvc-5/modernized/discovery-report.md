# Discovery report — vidly-mvc-5

Source: https://github.com/mosh-hamedani/vidly-mvc-5  
Clone (read-only): `runs/vidly-mvc-5/legacy`  
Inventory: `agent-config/scripts/inventory.ps1`

No stop-list match. This is one C# ASP.NET MVC 5 web application on .NET Framework 4.5. It is not a Web Site, not VB, not desktop, and not already SDK-style.

## Detected framework

| Project | TargetFrameworkVersion | SDK-style |
|---|---|---|
| `Vidly/Vidly.csproj` | `v4.5` | no |

`Web.config` compilation and `httpRuntime` also say `4.5`. MVC 5.2.2. Web API 5.2.2. Entity Framework 6.1.1. ASP.NET Identity 2.1 with OWIN cookie auth.

Counts from the script: 1 csproj, 0 aspx, 9 controllers, 0 svc, 0 asmx, 0 edmx. About 36 Razor views.

## Project structure

One web project, namespace `Vidly`. No class libraries and no solution file in the clone.

- `Controllers/` — MVC: Home, Account, Manage, Customers, Movies, Rentals
- `Controllers/Api/` — Web API: Customers, Movies, NewRentals
- `Models/` — EF6 entities plus Identity user
- `Dtos/` — API shapes
- `ViewModels/` — form view models
- `Views/` — Razor, layout in `Views/Shared/_Layout.cshtml`
- `Migrations/` — EF6 code-first migrations, including seed SQL
- `App_Start/` — routes, Web API, bundles, filters, AutoMapper, Identity
- `Global.asax.cs` and `Startup.cs` (OWIN)

## Controllers and actions

The inventory script lists MVC actions. It printed empty action lists for the three API controllers because those methods return `IHttpActionResult` or `IEnumerable<MovieDto>`, not `ActionResult`. The API names below are from the source.

MVC default route: `{controller}/{action}/{id}`, default `Home/Index`.  
API route: `api/{controller}/{id}`. JSON uses camel case.

### MVC

| Controller | Actions | Notes |
|---|---|---|
| `HomeController` | Index, About, Contact | Whole controller is `[AllowAnonymous]` |
| `AccountController` | Login (GET/POST), VerifyCode (GET/POST), Register (GET/POST), ConfirmEmail, ForgotPassword (GET/POST), ForgotPasswordConfirmation, ResetPassword (GET/POST), ResetPasswordConfirmation, ExternalLogin, SendCode (GET/POST), ExternalLoginCallback, ExternalLoginConfirmation (GET/POST), LogOff (POST), ExternalLoginFailure | Login and register flows are `[AllowAnonymous]`. Class also has `[Authorize]` |
| `ManageController` | Index, RemoveLogin (POST), AddPhoneNumber (GET/POST), EnableTwoFactorAuthentication (POST), DisableTwoFactorAuthentication (POST), VerifyPhoneNumber (GET/POST), RemovePhoneNumber (POST), ChangePassword (GET/POST), SetPassword (GET/POST), ManageLogins, LinkLogin (POST), LinkLoginCallback | `[Authorize]` |
| `CustomersController` | New, Save (POST), Index, Details, Edit | `New` and `Edit` both render `CustomerForm`. Index view loads rows from the API |
| `MoviesController` | Index, New, Edit, Details, Random, Save (POST) | Index renders `List` or `ReadOnlyList` from the role `CanManageMovies`. New, Edit, and Save require that role |
| `RentalsController` | New | View only. Saving a rental is the API |

### Web API

| Controller | Operations | Route shape |
|---|---|---|
| `Api/CustomersController` | GetCustomers, GetCustomer, CreateCustomer (POST), UpdateCustomer (PUT), DeleteCustomer (DELETE) | `/api/customers`, `/api/customers/{id}` |
| `Api/MoviesController` | GetMovies, GetMovie, CreateMovie (POST), UpdateMovie (PUT), DeleteMovie (DELETE) | `/api/movies`, `/api/movies/{id}` |
| `Api/NewRentalsController` | CreateNewRentals (POST) | `/api/newRentals` |

`GetMovies` returns only movies with `NumberAvailable > 0`. Create, update, and delete movie require role `CanManageMovies`. Customer API and new-rental API have no `[Authorize]`.

## Config (sanitized)

- Connection name `DefaultConnection`. LocalDB v11, database file under `|DataDirectory|`, integrated security. No password in the file.
- `sessionState` mode is `Off`.
- `<authentication mode="None" />`. Sign-in is OWIN cookies, login path `/Account/Login`. Cookie identity is rechecked every 30 minutes. Two-factor cookies are registered.
- Facebook login is enabled and reads `FacebookAppId` and `FacebookAppSecret` from `appSettings`. The values in source look like course placeholders. Microsoft, Twitter, and Google providers are commented out.
- `MailServer` appSetting is `dev-smtp.vidly.com`. `EmailService.SendAsync` does not send mail. `SmsService` does not send SMS.
- Custom errors: 404 goes to `~/404.html`.
- HTTP modules: Glimpse and Elmah. Glimpse endpoint `glimpse.axd` is on. Elmah page `elmah.axd` allows only `admin@vidly.com`.
- Global MVC filters in `FilterConfig`: `HandleError`, `Authorize`, `RequireHttps`.
- Bundles: stylesheet bundle `~/Content/css` includes `bootstrap-lumen.css`, `bootstrap-theme.css`, DataTables CSS, typeahead, toastr, and `site.css`. Layout calls `@Styles.Render("~/Content/css")`.
- Footer text: `Vidly`. Navbar brand: `Vidly`. Menu links: New Rental, Customers, Movies, plus the login partial.

## Data access

One EF6 context: `ApplicationDbContext : IdentityDbContext<ApplicationUser>`, connection name `DefaultConnection`.

DbSets: `Customers`, `Movies`, `MembershipTypes`, `Genres`, `Rentals`.

No `.edmx`. No stored procedures. No raw ADO.NET outside migrations. Controllers construct `new ApplicationDbContext()` themselves.

Entities:

- `Customer` — Name (required, 255), IsSubscribedToNewsletter, MembershipTypeId (byte), Birthdate (optional). Display labels: Membership Type, Date of Birth.
- `MembershipType` — Name, SignUpFee, DurationInMonths, DiscountRate. Constants: Unknown = 0, PayAsYouGo = 1.
- `Movie` — Name (required, 255), GenreId (required byte), DateAdded, ReleaseDate, NumberInStock (range 1–20), NumberAvailable. Display labels: Genre, Release Date, Number in Stock.
- `Genre` — Name (required, 255).
- `Rental` — Customer, Movie, DateRented, DateReturned (optional).
- `ApplicationUser` — Identity user plus DrivingLicense (required, 255) and Phone (required, 50).

Validation `Min18YearsIfAMember` on customer birthdate: Pay as You Go (id 1) and Unknown (id 0) skip the rule. Any other membership requires a birthdate and age of at least 18. Messages: `Birthdate is required.` and `Customer should be at least 18 years old to go on a membership.` Age is `today.Year - birthdate.Year` (no birthday-passed adjustment).

Migration seed SQL (not procedures):

- Membership types: Pay as You Go (0 fee, 0 months, 0%), Monthly (30, 1, 10%), Quarterly (90, 3, 15%), Annual (300, 12, 20%).
- Genres: Action, Thriller, Family, Romance, Comedy.
- `UPDATE Movies SET NumberAvailable = NumberInStock`.
- Two users: `admin@vidly.com` is in role `CanManageMovies`. `guest@vidly.com` is not. Password hashes are in the migration. The plaintext password is not in the repo. Those hashes are not copied here.

AutoMapper maps Customer, Movie, MembershipType, and Genre to DTOs. DTO-to-entity maps ignore `Id`.

## Call graph

```
Browser
  Home/Index, About, Contact          → views only (anonymous)
  Account/*, Manage/*                 → ASP.NET Identity / OWIN (users, roles, external login)
  Customers/Index                     → view → GET /api/customers, DELETE /api/customers/{id}
  Customers/New, Edit, Save, Details  → ApplicationDbContext.Customers, MembershipTypes
  Movies/Index                        → List or ReadOnlyList by role
       List / ReadOnlyList            → GET /api/movies, DELETE /api/movies/{id}
  Movies/New, Edit, Save, Details     → ApplicationDbContext.Movies, Genres
  Movies/Random                       → hardcoded Movie "Shrek!" and two fake customers, no database
  Rentals/New                         → view → GET /api/customers?query=, GET /api/movies?query=, POST /api/newRentals
  POST /api/newRentals                → Customers, Movies, Rentals
                                      → decrements Movie.NumberAvailable
                                      → rejects when NumberAvailable is 0 ("Movie is not available.")
```

Customers and movies are each reached from both an MVC controller and an API controller. Rentals are written only by the API. The rental screen reads customers and movies through the API.

## Services

No WCF and no ASMX. The in-app Web API is the only service surface. Consumers are the Razor pages above (DataTables and typeahead), not a separate client app.

## Cross-cutting concerns

- Global `Authorize` on MVC. Customers, movies, rentals, and account management require a signed-in user. Home does not. Web API customer and rental endpoints do not use that MVC filter.
- Global `RequireHttps`. A plain HTTP preview will redirect.
- OWIN cookie auth, external Facebook login, two-factor cookies. `HttpContext.GetOwinContext()` in Account and Manage. No `HttpContext.Current` found. No `Session` use. Session is off.
- Controllers new up `DbContext`. No DI container.
- Glimpse and Elmah HTTP modules.
- Newtonsoft.Json 6.0.4 for Web API.
- Static AutoMapper configuration in `Application_Start`.
- No background timers, no COM calls (only the default commented `AssemblyInfo` COM text), no registry use.

## Open questions and red flags

1. Seeded users are inserted before `DrivingLicense` and `Phone` are added as non-nullable columns with no default. A real migrate against those scripts can fail on existing rows. Plaintext passwords are not in source.
2. `RequireHttps` is global. Local HTTP preview needs a decision later (keep the redirect, or run HTTPS).
3. Movie stock changes only on rental create. `DateReturned` exists and is never set. There is no return-rental action.
4. `Movies/Random` is demo data, not the database.
5. Customer and movie delete exist on the API and on the list pages, not as MVC actions.
6. `MailServer` is configured and unused. Email and SMS identity services are empty.
7. Facebook keys in `appSettings` are placeholders. Do not treat them as real secrets, and do not copy them into the new app.
8. `inventory.ps1` does not list Web API actions. The names in this report are the ones to use later.
