# Scope proposal — vidly-mvc-5

First slice only. The other domains stay in the legacy app until a later run.

UI recommendation: ASP.NET Core MVC on `net10.0`. Same controller names, action names, URLs, and screen fields. A React rewrite happens only if the reply includes `use spa`.

## Ranked candidates

### 1. Customers — recommended

MVC `CustomersController` (New, Save, Index, Details, Edit) plus API `CustomersController` (get, get-by-id, create, update, delete). Membership types and the 18-year membership rule live here. The list page loads from `/api/customers`.

This is a full screen set with a real validation rule, and it is smaller than Accounts. The rental screen searches this API, so the new module must keep `/api/customers` and `/api/customers/{id}`. Movie stock and rental save stay out of this slice.

Blast radius: one shared database context. Navbar links for Movies and New Rental still have to appear. The customer pages sit behind the global sign-in filter, so the preview needs a way to open them. That does not mean rewriting Account and Manage.

### 2. Movies

MVC `MoviesController` (Index, New, Edit, Details, Random, Save) plus API `MoviesController`. Two list screens, switched by the role `CanManageMovies`. Genres are seed data in this domain. `Random` does not use the database.

Larger than Customers because of the role split and because rentals decrement `NumberAvailable` through the movie API. A Movies-only slice still has to keep `/api/movies` for the rental screen.

Blast radius: the role lives in Accounts. Stock changes when a rental is created, which is outside this slice.

### 3. Rentals

One MVC action, `RentalsController.New`, and one API action, `NewRentalsController.CreateNewRentals`. The screen is small. The behavior is not: it searches customers, searches movies with stock, writes rentals, and decrements `NumberAvailable`.

A bad first slice. Building it means also standing up customer and movie data, or the form cannot be used.

### 4. Accounts

`AccountController`, `ManageController`, Identity, OWIN cookies, Facebook login, two-factor, and empty email/SMS senders. This is the largest and most template-driven area. It is a poor first proof of the pipeline.

Home is not on this shortlist. It is three anonymous template pages and does not exercise the video-store behavior.
