# Test report — Customers

## Commands

`dotnet test runs/vidly-mvc-5/modernized/VidlyCustomers.Tests/VidlyCustomers.Tests.csproj --nologo`

```
  Determining projects to restore...
  All projects are up-to-date for restore.
  VidlyCustomers -> runs/vidly-mvc-5/modernized/VidlyCustomers/bin/Debug/net10.0/VidlyCustomers.dll
  VidlyCustomers.Tests -> runs/vidly-mvc-5/modernized/VidlyCustomers.Tests/bin/Debug/net10.0/VidlyCustomers.Tests.dll
Test run for VidlyCustomers.Tests.dll (.NETCoreApp,Version=v10.0)
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    31, Skipped:     0, Total:    31, Duration: 1 s - VidlyCustomers.Tests.dll (net10.0)
```

`compare-parity.ps1 -LegacyRoot runs/vidly-mvc-5/legacy/Vidly -ModernRoot runs/vidly-mvc-5/modernized/VidlyCustomers -ParityMap runs/vidly-mvc-5/modernized/parity-map.md -BaseUrl http://127.0.0.1:5188`

Result: `=== PARITY PASSED ===`. Stylesheet `http://127.0.0.1:5188/css/site.css` loaded (1242 bytes).

## Scenario coverage

Every `Scenario:` title in `backlog.md` is the display name of one passing test in `VidlyCustomers.Tests/CustomerScenarioTests.cs`. None are missing.

| Scenario | Test |
|---|---|
| Customer list shows the heading, the New Customer link, and the column headers | CustomerScenarioTests.CustomerListShowsHeading |
| Customer list page calls the customer API for rows | CustomerScenarioTests.CustomerListCallsApi |
| New customer form shows the membership fields and Save | CustomerScenarioTests.NewCustomerFormShowsFields |
| Save a new pay-as-you-go customer with no birthdate | CustomerScenarioTests.SavePayAsYouGoWithoutBirthdate |
| Save rejects a missing name | CustomerScenarioTests.SaveRejectsMissingName |
| Save rejects an empty membership type | CustomerScenarioTests.SaveRejectsEmptyMembership |
| Save rejects a membership when the birthdate is missing | CustomerScenarioTests.SaveRejectsMissingBirthdate |
| Save rejects a membership when the customer is under 18 | CustomerScenarioTests.SaveRejectsUnder18 |
| Save accepts a membership when the year difference is 18 | CustomerScenarioTests.SaveAcceptsYearDifference18 |
| Save rejects a name longer than 255 characters | CustomerScenarioTests.SaveRejectsLongName |
| Edit shows the New Customer form for an existing customer | CustomerScenarioTests.EditShowsForm |
| Edit of an unknown customer returns not found | CustomerScenarioTests.EditUnknownIsNotFound |
| Save updates only name, birthdate, membership type, and newsletter flag | CustomerScenarioTests.SaveUpdatesFourFields |
| Save of an unknown customer id fails | CustomerScenarioTests.SaveUnknownIdFails |
| Details shows the name, membership type, and birthdate | CustomerScenarioTests.DetailsShowsBirthdate |
| Details omits the birthdate line when the birthdate is empty | CustomerScenarioTests.DetailsOmitsEmptyBirthdate |
| Details of an unknown customer returns not found | CustomerScenarioTests.DetailsUnknownIsNotFound |
| Customer list includes the delete confirmation text | CustomerScenarioTests.CustomerListIncludesDeleteConfirm |
| Delete customer removes that customer | CustomerScenarioTests.DeleteRemovesCustomer |
| Delete of an unknown customer returns not found | CustomerScenarioTests.DeleteUnknownIsNotFound |
| Customer API returns a JSON array | CustomerScenarioTests.ApiReturnsArray |
| Customer API filters by a name fragment | CustomerScenarioTests.ApiFiltersByName |
| Customer API returns one customer | CustomerScenarioTests.ApiReturnsOneCustomer |
| Customer API returns not found for an unknown id | CustomerScenarioTests.ApiGetUnknownIsNotFound |
| Customer API creates a customer | CustomerScenarioTests.ApiCreatesCustomer |
| Customer API rejects a create with no name | CustomerScenarioTests.ApiRejectsCreateWithoutName |
| Customer API updates a customer | CustomerScenarioTests.ApiUpdatesCustomer |
| Customer API returns not found when updating an unknown id | CustomerScenarioTests.ApiUpdateUnknownIsNotFound |
| Customer API rejects an update with no name | CustomerScenarioTests.ApiRejectsUpdateWithoutName |
| Customer API does not apply the 18-year membership rule | CustomerScenarioTests.ApiSkipsAgeRule |
| Customer pages show the Vidly menu and footer | CustomerScenarioTests.CustomerPagesShowChrome |

## Gaps still in force from the approved plan

- Customer pages do not redirect to login.
- The preview is HTTP and does not force HTTPS.
- Data source: preview data is fake. John Smith and Mary Williams are not legacy database rows.
