# Implementation plan — Customers

## Module
Customers. MVC `CustomersController` and Web API `CustomersController`. Gate 2 approved this controller slice as ASP.NET Core MVC.

## Target project structure
```
runs/vidly-mvc-5/modernized/
  discovery-report.md
  knowledge-graph.md
  scope-proposal.md
  backlog.md
  plan.md
  parity-map.md
  VidlyCustomers/
    VidlyCustomers.csproj          net10.0
    Program.cs
    appsettings.json
    Controllers/CustomersController.cs
    Controllers/Api/CustomersController.cs
    Models/
    Dtos/
    ViewModels/
    Services/CustomerService.cs
    Views/Customers/
    Views/Shared/_Layout.cshtml
    Views/Shared/_NavBar.cshtml
    Views/Shared/_LoginPartial.cshtml
    wwwroot/css/site.css
  VidlyCustomers.Tests/
    VidlyCustomers.Tests.csproj    sibling of the web project
```

## Build order
1. Data layer — `Customer`, `MembershipType`, and `CustomerDto` / `MembershipTypeDto` with the same property names. In-memory store for this preview. No EF Core migration and no connection to the legacy LocalDB file.
2. Service layer — `CustomerService` holds the list, the save rules, the 18-year check, and the API create/update/delete rules from `backlog.md`.
3. Controllers — same class names, action names, and verbs as `parity-map.md`. MVC `Save` stays POST with the antiforgery token. API routes stay `/api/customers` and `/api/customers/{id}`.
4. Views — Razor. Copy the customer form, list, details, layout, navbar, and anonymous login partial. Default route stays `{controller}/{action}/{id}` with Home/Index as the default, so customer URLs stay `/Customers/...`.

## Technology mapping applied
- `net10.0`, SDK-style project, `Program.cs`, nullable reference types.
- Inject `CustomerService`. Controllers do not construct a database context.
- Async actions for the service calls.
- Membership type names live in configuration or in the service seed from the migration SQL, not as scattered literals in the views.
- JSON stays camel case for the API. `Newtonsoft.Json` is not required if `System.Text.Json` emits the same camel-case names.
- No `HttpContext.Current`.
- Logging through `Microsoft.Extensions.Logging`.

## Data migration notes
None required. This run does not change a database.

Membership types copied from the legacy migration SQL:

| Id | Name | SignUpFee | DurationInMonths | DiscountRate |
|---|---|---|---|---|
| 1 | Pay as You Go | 0 | 0 | 0 |
| 2 | Monthly | 30 | 1 | 10 |
| 3 | Quarterly | 90 | 3 | 15 |
| 4 | Annual | 300 | 12 | 20 |

Data source for customer rows, if this plan is approved: preview data is fake. Proposed rows so Details and Edit have a page to render:

| Id | Name | MembershipTypeId | Birthdate | IsSubscribedToNewsletter |
|---|---|---|---|---|
| 1 | John Smith | 2 | 1980-01-01 | true |
| 2 | Mary Williams | 1 | empty | false |

These two people are not in the legacy database. The legacy migrations do not seed customers.

## Open questions / ambiguous behavior
1. Global `Authorize` sends an anonymous customer page to `/Account/Login`. Account is out of this slice. The parity check GETs the customer URL and looks for the customer text. Proposed gap for this run: customer pages render without a sign-in redirect, and the anonymous menu shows "Register" and "Log in". Accept this gap, or keep the redirect and leave the rendered check blocked until Accounts is in scope.
2. Global `RequireHttps` redirects HTTP to HTTPS. Proposed gap for this run: the preview listens on HTTP and does not force HTTPS.
3. `CustomersController.Save` uses `Single` when the id is not 0. A missing id throws. It does not return not found. The scenario says the save fails and no customer is created. The HTTP status of that failure is the framework error page, not a coded 404.
4. Age is year minus year. A birthday later this year still counts as already that age.
5. `CustomerDto` has the 18-year attribute commented out. The API stores an under-18 member. MVC Save does not.
6. API name filter is `Contains` with no `ToLower`.
7. `Details` reads `MembershipType.Name` with no null check. A customer whose membership type id has no row would throw. The four seeded types avoid that for the preview. No scenario covers the throw.
8. Delete does not check rentals. Rentals are out of scope. The preview has no rental rows, so delete of a preview customer succeeds.
9. AutoMapper maps `CustomerDto` onto `Customer` and ignores `Id`. A nested `membershipType` object on an update was not executed in this phase. The scenarios cover name, validation, and not-found only.

## Out of scope for this run
Movies, Rentals, Account, Manage, Home, Facebook login, two-factor, email and SMS senders, Glimpse, Elmah, and the authenticated "Hello" / "Log off" partial.
