# Parity map (`parity-map.md`)

## Module
Customers

## Project type
MVC and Web API

## Target
`net10.0`

## Controllers and actions

| Old file | Old type | Action | Verb | Old URL or page | New file | New action | New URL |
|---|---|---|---|---|---|---|---|
| `Controllers/CustomersController.cs` | MVC | `Index` | GET | `/Customers/Index` | `Controllers/CustomersController.cs` | `Index` | `/Customers/Index` |
| `Controllers/CustomersController.cs` | MVC | `New` | GET | `/Customers/New` | `Controllers/CustomersController.cs` | `New` | `/Customers/New` |
| `Controllers/CustomersController.cs` | MVC | `Edit` | GET | `/Customers/Edit/{id}` | `Controllers/CustomersController.cs` | `Edit` | `/Customers/Edit/{id}` |
| `Controllers/CustomersController.cs` | MVC | `Details` | GET | `/Customers/Details/{id}` | `Controllers/CustomersController.cs` | `Details` | `/Customers/Details/{id}` |
| `Controllers/CustomersController.cs` | MVC | `Save` | POST | `/Customers/Save` | `Controllers/CustomersController.cs` | `Save` | `/Customers/Save` |
| `Controllers/Api/CustomersController.cs` | Web API | `GetCustomers` | GET | `/api/customers` | `Controllers/Api/CustomersController.cs` | `GetCustomers` | `/api/customers` |
| `Controllers/Api/CustomersController.cs` | Web API | `GetCustomer` | GET | `/api/customers/{id}` | `Controllers/Api/CustomersController.cs` | `GetCustomer` | `/api/customers/{id}` |
| `Controllers/Api/CustomersController.cs` | Web API | `CreateCustomer` | POST | `/api/customers` | `Controllers/Api/CustomersController.cs` | `CreateCustomer` | `/api/customers` |
| `Controllers/Api/CustomersController.cs` | Web API | `UpdateCustomer` | PUT | `/api/customers/{id}` | `Controllers/Api/CustomersController.cs` | `UpdateCustomer` | `/api/customers/{id}` |
| `Controllers/Api/CustomersController.cs` | Web API | `DeleteCustomer` | DELETE | `/api/customers/{id}` | `Controllers/Api/CustomersController.cs` | `DeleteCustomer` | `/api/customers/{id}` |

## Shared chrome

| Location | Kind | Text that must appear | Page URL |
|---|---|---|---|
| Layout title | title | My ASP.NET Application | `/Customers/Index` |
| Layout navbar | link | Vidly | `/Customers/Index` |
| Layout navbar | link | New Rental | `/Customers/Index` |
| Layout navbar | link | Customers | `/Customers/Index` |
| Layout navbar | link | Movies | `/Customers/Index` |
| Layout navbar | link | Register | `/Customers/Index` |
| Layout navbar | link | Log in | `/Customers/Index` |
| Layout footer | footer | Vidly | `/Customers/Index` |

## Screen fields

| Screen | Old control | Kind | Text that must appear | Page URL |
|---|---|---|---|---|
| Customer Index | heading | heading | Customers | `/Customers/Index` |
| Customer Index | create link | link | New Customer | `/Customers/Index` |
| Customer Index | column | column | Customer | `/Customers/Index` |
| Customer Index | column | column | Membership Type | `/Customers/Index` |
| Customer Index | column | column | Delete | `/Customers/Index` |
| Customer Index | delete confirm | text | Are you sure you want to delete this customer? | `/Customers/Index` |
| Customer Index | api url | text | /api/customers | `/Customers/Index` |
| Customer form | heading | heading | New Customer | `/Customers/New` |
| Customer form | label | label | Name | `/Customers/New` |
| Customer form | input | input | Customer.Name | `/Customers/New` |
| Customer form | label | label | Membership Type | `/Customers/New` |
| Customer form | input | input | Customer.MembershipTypeId | `/Customers/New` |
| Customer form | option | option | Pay as You Go | `/Customers/New` |
| Customer form | option | option | Monthly | `/Customers/New` |
| Customer form | option | option | Quarterly | `/Customers/New` |
| Customer form | option | option | Annual | `/Customers/New` |
| Customer form | label | label | Date of Birth | `/Customers/New` |
| Customer form | input | input | Customer.Birthdate | `/Customers/New` |
| Customer form | checkbox | text | Subscribed to Newsletter? | `/Customers/New` |
| Customer form | input | input | Customer.IsSubscribedToNewsletter | `/Customers/New` |
| Customer form | input | input | Customer.Id | `/Customers/New` |
| Customer form | button | button | Save | `/Customers/New` |
| Customer form | summary | text | Please fix the following errors. | `/Customers/New` |
| Customer Details | label | text | Membership Type: | `/Customers/Details/1` |
| Customer Details | label | text | Birthdate: | `/Customers/Details/1` |
| Customer Edit | heading | heading | New Customer | `/Customers/Edit/1` |

## Out of scope on purpose
Movies, Rentals, Account, Manage, and Home actions. Authenticated navbar text "Log off" and "Hello". Glimpse and Elmah. The 404.html page.
