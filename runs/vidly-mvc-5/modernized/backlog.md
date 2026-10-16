# Backlog — Customers

Status: ready to merge. Gate 4 approved. The Customers module is complete for this run.

Scope from Gate 2: `CustomersController` and `Api/CustomersController`. ASP.NET Core MVC. No React.

Legacy sources: `Controllers/CustomersController.cs`, `Controllers/Api/CustomersController.cs`, `Models/Customer.cs`, `Models/Min18YearsIfAMember.cs`, `Models/MembershipType.cs`, `Dtos/CustomerDto.cs`, `Views/Customers/*`, `Views/Shared/_Layout.cshtml`, `Views/Shared/_NavBar.cshtml`, `Views/Shared/_LoginPartial.cshtml`.

Membership type rows used below come from the migration SQL: 1 Pay as You Go, 2 Monthly, 3 Quarterly, 4 Annual.

---

**Story ID:** US-001
**Title:** Customer list

**As a** store clerk
**I want to** open the customer list
**So that** I can see customers and add one

**Legacy source reference(s):** `CustomersController.Index`, `Views/Customers/Index.cshtml`

**Acceptance criteria:**

```gherkin
Feature: US-001 Customer list
  As a store clerk
  I want to open the customer list
  So that I can see customers and add one

  Scenario: Customer list shows the heading, the New Customer link, and the column headers
    Given the customer list is open
    When the page is shown
    Then the heading is "Customers"
    And the page links "New Customer" to the New action
    And the table headers are "Customer", "Membership Type", and "Delete"

  Scenario: Customer list page calls the customer API for rows
    Given the customer list is open
    When the page is shown
    Then the page script requests "/api/customers"
    And a customer name is rendered as a link to "/customers/edit/{id}"
```

---

**Story ID:** US-002
**Title:** New customer form

**As a** store clerk
**I want to** open the new customer form
**So that** I can enter a customer

**Legacy source reference(s):** `CustomersController.New`, `Views/Customers/CustomerForm.cshtml`

**Acceptance criteria:**

```gherkin
Feature: US-002 New customer form
  As a store clerk
  I want to open the new customer form
  So that I can enter a customer

  Scenario: New customer form shows the membership fields and Save
    Given no customer is selected
    When the clerk opens New
    Then the title is "New"
    And the heading is "New Customer"
    And the form posts to Save
    And the labels are "Name", "Membership Type", and "Date of Birth"
    And the checkbox text is "Subscribed to Newsletter?"
    And the button text is "Save"
    And the membership list includes "Pay as You Go", "Monthly", "Quarterly", and "Annual"
    And the input names are "Customer.Name", "Customer.MembershipTypeId", "Customer.Birthdate", "Customer.IsSubscribedToNewsletter", and "Customer.Id"
```

---

**Story ID:** US-003
**Title:** Save a new customer

**As a** store clerk
**I want to** save a new customer
**So that** the customer is stored

**Legacy source reference(s):** `CustomersController.Save`, `Customer`, `Min18YearsIfAMember`

**Notes / ambiguities found:** Age is `DateTime.Today.Year - birthdate.Year`. The code does not check whether the birthday has already happened this year.

**Acceptance criteria:**

```gherkin
Feature: US-003 Save a new customer
  As a store clerk
  I want to save a new customer
  So that the customer is stored

  Scenario: Save a new pay-as-you-go customer with no birthdate
    Given membership type 1 is Pay as You Go
    And the posted customer id is 0
    And the name is present and at most 255 characters
    When the clerk posts Save
    Then the customer is added
    And the response redirects to the customer Index action

  Scenario: Save rejects a missing name
    Given the posted name is empty
    When the clerk posts Save
    Then the customer is not added
    And the response shows "Please fix the following errors."
    And the response shows "The Name field is required."

  Scenario: Save rejects an empty membership type
    Given the posted membership type is blank
    When the clerk posts Save
    Then the customer is not added
    And the response shows "The value '' is invalid."

  Scenario: Save rejects a membership when the birthdate is missing
    Given the membership type is Monthly, Quarterly, or Annual
    And the birthdate is empty
    When the clerk posts Save
    Then the customer is not added
    And the response shows "Birthdate is required."

  Scenario: Save rejects a membership when the customer is under 18
    Given the membership type is Monthly, Quarterly, or Annual
    And the birth year is 17 years before today
    When the clerk posts Save
    Then the customer is not added
    And the response shows "Customer should be at least 18 years old to go on a membership."

  Scenario: Save accepts a membership when the year difference is 18
    Given the membership type is Monthly
    And the birth year is 18 years before today
    When the clerk posts Save
    Then the customer is added
    And the age check uses year minus year only

  Scenario: Save rejects a name longer than 255 characters
    Given the posted name is 256 characters
    When the clerk posts Save
    Then the customer is not added
    And the response shows "The field Name must be a string or array type with a maximum length of '255'."
```

---

**Story ID:** US-004
**Title:** Edit a customer

**As a** store clerk
**I want to** edit an existing customer
**So that** the stored fields change

**Legacy source reference(s):** `CustomersController.Edit`, `CustomersController.Save`

**Notes / ambiguities found:** Edit uses the same `CustomerForm` view as New. The heading stays "New Customer". Save of an id that is not 0 calls `Single` and throws when that id is missing. It does not return not found.

**Acceptance criteria:**

```gherkin
Feature: US-004 Edit a customer
  As a store clerk
  I want to edit an existing customer
  So that the stored fields change

  Scenario: Edit shows the New Customer form for an existing customer
    Given a customer exists
    When the clerk opens Edit for that id
    Then the response is the CustomerForm view
    And the heading is "New Customer"
    And the title is "New"
    And the form shows that customer's name

  Scenario: Edit of an unknown customer returns not found
    Given no customer has the requested id
    When the clerk opens Edit
    Then the response is not found

  Scenario: Save updates only name, birthdate, membership type, and newsletter flag
    Given a customer exists
    When the clerk posts Save with that id and new values for those four fields
    Then those four fields are stored
    And the response redirects to the customer Index action

  Scenario: Save of an unknown customer id fails
    Given the posted id is not 0
    And no customer has that id
    When the clerk posts Save
    Then the save fails
    And no customer is created
```

---

**Story ID:** US-005
**Title:** Customer details

**As a** store clerk
**I want to** open one customer
**So that** I can read the membership and birthdate

**Legacy source reference(s):** `CustomersController.Details`, `Views/Customers/Details.cshtml`

**Acceptance criteria:**

```gherkin
Feature: US-005 Customer details
  As a store clerk
  I want to open one customer
  So that I can read the membership and birthdate

  Scenario: Details shows the name, membership type, and birthdate
    Given a customer has a membership type and a birthdate
    When the clerk opens Details
    Then the heading is the customer name
    And the page shows "Membership Type: " followed by the membership name
    And the page shows "Birthdate: " followed by the short date

  Scenario: Details omits the birthdate line when the birthdate is empty
    Given a customer has no birthdate
    When the clerk opens Details
    Then the page shows the membership type
    And the page does not show "Birthdate:"

  Scenario: Details of an unknown customer returns not found
    Given no customer has the requested id
    When the clerk opens Details
    Then the response is not found
```

---

**Story ID:** US-006
**Title:** Delete a customer

**As a** store clerk
**I want to** delete a customer from the list
**So that** the customer is removed

**Legacy source reference(s):** `Views/Customers/Index.cshtml`, `Api/CustomersController.DeleteCustomer`

**Notes / ambiguities found:** There is no MVC delete action. The list button calls `DELETE /api/customers/{id}`. The code removes the row and does not look at rentals first.

**Acceptance criteria:**

```gherkin
Feature: US-006 Delete a customer
  As a store clerk
  I want to delete a customer from the list
  So that the customer is removed

  Scenario: Customer list includes the delete confirmation text
    Given the customer list is open
    When the page is shown
    Then the page includes "Are you sure you want to delete this customer?"
    And the page includes a Delete button for a customer id

  Scenario: Delete customer removes that customer
    Given a customer exists
    When the client sends DELETE to /api/customers/{id}
    Then that customer is removed
    And the response is success

  Scenario: Delete of an unknown customer returns not found
    Given no customer has the requested id
    When the client sends DELETE to /api/customers/{id}
    Then the response is not found
```

---

**Story ID:** US-007
**Title:** Read customers from the API

**As a** list or rental screen
**I want to** read customers from the API
**So that** the screen can show names

**Legacy source reference(s):** `Api/CustomersController.GetCustomers`, `GetCustomer`

**Notes / ambiguities found:** `Contains` is called with no `ToLower`. JSON property names are camel case.

**Acceptance criteria:**

```gherkin
Feature: US-007 Read customers from the API
  As a list or rental screen
  I want to read customers from the API
  So that the screen can show names

  Scenario: Customer API returns a JSON array
    Given customers exist
    When the client sends GET /api/customers
    Then the response is a JSON array
    And each item includes "id", "name", and "membershipType"

  Scenario: Customer API filters by a name fragment
    Given customers named "John Smith" and "Mary Williams" exist
    When the client sends GET /api/customers?query=John
    Then the response includes "John Smith"
    And the response excludes "Mary Williams"
    And the filter is Contains with no ToLower

  Scenario: Customer API returns one customer
    Given a customer exists
    When the client sends GET /api/customers/{id}
    Then the response is that customer

  Scenario: Customer API returns not found for an unknown id
    Given no customer has the requested id
    When the client sends GET /api/customers/{id}
    Then the response is not found
```

---

**Story ID:** US-008
**Title:** Create and update a customer from the API

**As a** client of the customer API
**I want to** create or update a customer
**So that** the stored customer matches the posted fields

**Legacy source reference(s):** `Api/CustomersController.CreateCustomer`, `UpdateCustomer`, `Dtos/CustomerDto.cs`

**Notes / ambiguities found:** `[Min18YearsIfAMember]` is commented out on `CustomerDto`. The API does not run that rule. MVC Save does.

**Acceptance criteria:**

```gherkin
Feature: US-008 Create and update a customer from the API
  As a client of the customer API
  I want to create or update a customer
  So that the stored customer matches the posted fields

  Scenario: Customer API creates a customer
    Given the posted name is present
    When the client sends POST /api/customers
    Then the customer is stored
    And the response is created
    And the body includes the new id

  Scenario: Customer API rejects a create with no name
    Given the posted name is empty
    When the client sends POST /api/customers
    Then the customer is not stored
    And the response is bad request

  Scenario: Customer API updates a customer
    Given a customer exists
    When the client sends PUT /api/customers/{id} with a new name
    Then the stored name changes
    And the response is success

  Scenario: Customer API returns not found when updating an unknown id
    Given no customer has the requested id
    When the client sends PUT /api/customers/{id}
    Then the response is not found

  Scenario: Customer API rejects an update with no name
    Given a customer exists
    And the posted name is empty
    When the client sends PUT /api/customers/{id}
    Then the stored name does not change
    And the response is bad request

  Scenario: Customer API does not apply the 18-year membership rule
    Given the membership type is Monthly
    And the birth year is 17 years before today
    When the client sends POST /api/customers
    Then the customer is stored
```

---

**Story ID:** US-009
**Title:** Customer page chrome

**As a** store clerk
**I want to** see the same menu and footer on customer pages
**So that** the screen is still Vidly

**Legacy source reference(s):** `Views/Shared/_Layout.cshtml`, `_NavBar.cshtml`, `_LoginPartial.cshtml`

**Notes / ambiguities found:** The login partial shows "Register" and "Log in" only when the request is anonymous. See the open question in `plan.md` about the global sign-in filter.

**Acceptance criteria:**

```gherkin
Feature: US-009 Customer page chrome
  As a store clerk
  I want to see the same menu and footer on customer pages
  So that the screen is still Vidly

  Scenario: Customer pages show the Vidly menu and footer
    Given a customer page is shown without a signed-in user
    When the page is rendered
    Then the page shows "Vidly"
    And the page shows "New Rental", "Customers", and "Movies"
    And the page shows "Register" and "Log in"
    And the page title ends with "My ASP.NET Application"
    And the page links a stylesheet
```
