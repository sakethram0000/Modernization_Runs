using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using VidlyCustomers.Services;

namespace VidlyCustomers.Tests;

[CollectionDefinition("customers", DisableParallelization = true)]
public sealed class CustomerCollection : ICollectionFixture<CustomerAppFactory>
{
}

public class CustomerAppFactory : WebApplicationFactory<Program>
{
}

[Collection("customers")]
public class CustomerScenarioTests
{
    private readonly HttpClient _client;
    private readonly CustomerStore _store;

    public CustomerScenarioTests(CustomerAppFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        _store = factory.Services.GetRequiredService<CustomerStore>();
        _store.Reset();
    }

    [Fact(DisplayName = "Customer list shows the heading, the New Customer link, and the column headers")]
    public async Task CustomerListShowsHeading()
    {
        var html = await GetHtmlAsync("/Customers/Index");

        Assert.Contains("<h2>Customers</h2>", html);
        Assert.Contains("New Customer", html);
        Assert.Contains(">Customer<", html);
        Assert.Contains(">Membership Type<", html);
        Assert.Contains(">Delete<", html);
    }

    [Fact(DisplayName = "Customer list page calls the customer API for rows")]
    public async Task CustomerListCallsApi()
    {
        var html = await GetHtmlAsync("/Customers/Index");
        var api = await _client.GetAsync("/api/customers");
        var json = await api.Content.ReadAsStringAsync();

        Assert.Contains("\"/api/customers\"", html);
        Assert.Contains("/customers/edit/", html);
        Assert.Contains("customer.name", html);
        Assert.Contains("John Smith", json);
    }

    [Fact(DisplayName = "New customer form shows the membership fields and Save")]
    public async Task NewCustomerFormShowsFields()
    {
        var html = await GetHtmlAsync("/Customers/New");

        Assert.Contains("<title>New - My ASP.NET Application</title>", html);
        Assert.Contains("<h2>New Customer</h2>", html);
        Assert.Contains("action=\"/Customers/Save\"", html);
        Assert.Contains("Name", html);
        Assert.Contains("Membership Type", html);
        Assert.Contains("Date of Birth", html);
        Assert.Contains("Subscribed to Newsletter?", html);
        Assert.Contains(">Save<", html);
        Assert.Contains("Pay as You Go", html);
        Assert.Contains("Monthly", html);
        Assert.Contains("Quarterly", html);
        Assert.Contains("Annual", html);
        Assert.Contains("name=\"Customer.Name\"", html);
        Assert.Contains("name=\"Customer.MembershipTypeId\"", html);
        Assert.Contains("name=\"Customer.Birthdate\"", html);
        Assert.Contains("name=\"Customer.IsSubscribedToNewsletter\"", html);
        Assert.Contains("name=\"Customer.Id\"", html);
    }

    [Fact(DisplayName = "Save a new pay-as-you-go customer with no birthdate")]
    public async Task SavePayAsYouGoWithoutBirthdate()
    {
        var response = await PostSaveAsync("/Customers/New", new Dictionary<string, string>
        {
            ["Customer.Id"] = "0",
            ["Customer.Name"] = "Ada Lovelace",
            ["Customer.MembershipTypeId"] = "1",
            ["Customer.IsSubscribedToNewsletter"] = "false"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Customers", response.Headers.Location?.OriginalString);
        var created = _store.Find(3);
        Assert.NotNull(created);
        Assert.Equal("Ada Lovelace", created.Name);
        Assert.Null(created.Birthdate);
    }

    [Fact(DisplayName = "Save rejects a missing name")]
    public async Task SaveRejectsMissingName()
    {
        var html = await PostSaveHtmlAsync(new Dictionary<string, string>
        {
            ["Customer.Id"] = "0",
            ["Customer.Name"] = "",
            ["Customer.MembershipTypeId"] = "1"
        });

        Assert.Contains("Please fix the following errors.", html);
        Assert.Contains("The Name field is required.", html);
        Assert.Equal(2, _store.Count);
    }

    [Fact(DisplayName = "Save rejects an empty membership type")]
    public async Task SaveRejectsEmptyMembership()
    {
        var html = await PostSaveHtmlAsync(new Dictionary<string, string>
        {
            ["Customer.Id"] = "0",
            ["Customer.Name"] = "No Membership",
            ["Customer.MembershipTypeId"] = ""
        });

        Assert.Contains("The value '' is invalid.", html);
        Assert.Equal(2, _store.Count);
    }

    [Fact(DisplayName = "Save rejects a membership when the birthdate is missing")]
    public async Task SaveRejectsMissingBirthdate()
    {
        var html = await PostSaveHtmlAsync(new Dictionary<string, string>
        {
            ["Customer.Id"] = "0",
            ["Customer.Name"] = "Monthly Person",
            ["Customer.MembershipTypeId"] = "2"
        });

        Assert.Contains("Birthdate is required.", html);
        Assert.Equal(2, _store.Count);
    }

    [Fact(DisplayName = "Save rejects a membership when the customer is under 18")]
    public async Task SaveRejectsUnder18()
    {
        var html = await PostSaveHtmlAsync(new Dictionary<string, string>
        {
            ["Customer.Id"] = "0",
            ["Customer.Name"] = "Young Member",
            ["Customer.MembershipTypeId"] = "2",
            ["Customer.Birthdate"] = $"{DateTime.Today.Year - 17}-06-15"
        });

        Assert.Contains("Customer should be at least 18 years old to go on a membership.", html);
        Assert.Equal(2, _store.Count);
    }

    [Fact(DisplayName = "Save accepts a membership when the year difference is 18")]
    public async Task SaveAcceptsYearDifference18()
    {
        var response = await PostSaveAsync("/Customers/New", new Dictionary<string, string>
        {
            ["Customer.Id"] = "0",
            ["Customer.Name"] = "Just Eighteen",
            ["Customer.MembershipTypeId"] = "2",
            ["Customer.Birthdate"] = $"{DateTime.Today.Year - 18}-12-31"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var created = _store.Find(3);
        Assert.NotNull(created);
        Assert.Equal(18, DateTime.Today.Year - created.Birthdate!.Value.Year);
    }

    [Fact(DisplayName = "Save rejects a name longer than 255 characters")]
    public async Task SaveRejectsLongName()
    {
        var html = await PostSaveHtmlAsync(new Dictionary<string, string>
        {
            ["Customer.Id"] = "0",
            ["Customer.Name"] = new string('a', 256),
            ["Customer.MembershipTypeId"] = "1"
        });

        Assert.Contains("The field Name must be a string or array type with a maximum length of '255'.", html);
        Assert.Equal(2, _store.Count);
    }

    [Fact(DisplayName = "Edit shows the New Customer form for an existing customer")]
    public async Task EditShowsForm()
    {
        var html = await GetHtmlAsync("/Customers/Edit/1");

        Assert.Contains("<title>New - My ASP.NET Application</title>", html);
        Assert.Contains("<h2>New Customer</h2>", html);
        Assert.Contains("value=\"John Smith\"", html);
    }

    [Fact(DisplayName = "Edit of an unknown customer returns not found")]
    public async Task EditUnknownIsNotFound()
    {
        var response = await _client.GetAsync("/Customers/Edit/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "Save updates only name, birthdate, membership type, and newsletter flag")]
    public async Task SaveUpdatesFourFields()
    {
        var response = await PostSaveAsync("/Customers/Edit/1", new Dictionary<string, string>
        {
            ["Customer.Id"] = "1",
            ["Customer.Name"] = "John Updated",
            ["Customer.MembershipTypeId"] = "4",
            ["Customer.Birthdate"] = "1975-05-05",
            ["Customer.IsSubscribedToNewsletter"] = "false"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var updated = _store.Find(1);
        Assert.NotNull(updated);
        Assert.Equal("John Updated", updated.Name);
        Assert.Equal(new DateTime(1975, 5, 5), updated.Birthdate);
        Assert.Equal((byte)4, updated.MembershipTypeId);
        Assert.False(updated.IsSubscribedToNewsletter);
    }

    [Fact(DisplayName = "Save of an unknown customer id fails")]
    public async Task SaveUnknownIdFails()
    {
        var response = await PostSaveAsync("/Customers/New", new Dictionary<string, string>
        {
            ["Customer.Id"] = "999",
            ["Customer.Name"] = "Missing",
            ["Customer.MembershipTypeId"] = "1"
        });

        Assert.True((int)response.StatusCode >= 500);
        Assert.Equal(2, _store.Count);
        Assert.Null(_store.Find(999));
    }

    [Fact(DisplayName = "Details shows the name, membership type, and birthdate")]
    public async Task DetailsShowsBirthdate()
    {
        var html = await GetHtmlAsync("/Customers/Details/1");

        Assert.Contains("<h2>John Smith</h2>", html);
        Assert.Contains("Membership Type: Monthly", html);
        Assert.Contains("Birthdate: 1/1/1980", html);
    }

    [Fact(DisplayName = "Details omits the birthdate line when the birthdate is empty")]
    public async Task DetailsOmitsEmptyBirthdate()
    {
        var html = await GetHtmlAsync("/Customers/Details/2");

        Assert.Contains("Membership Type: Pay as You Go", html);
        Assert.DoesNotContain("Birthdate:", html);
    }

    [Fact(DisplayName = "Details of an unknown customer returns not found")]
    public async Task DetailsUnknownIsNotFound()
    {
        var response = await _client.GetAsync("/Customers/Details/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "Customer list includes the delete confirmation text")]
    public async Task CustomerListIncludesDeleteConfirm()
    {
        var html = await GetHtmlAsync("/Customers/Index");

        Assert.Contains("Are you sure you want to delete this customer?", html);
        Assert.Contains("data-customer-id", html);
        Assert.Contains(">Delete<", html);
    }

    [Fact(DisplayName = "Delete customer removes that customer")]
    public async Task DeleteRemovesCustomer()
    {
        var response = await _client.DeleteAsync("/api/customers/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(_store.Find(1));
    }

    [Fact(DisplayName = "Delete of an unknown customer returns not found")]
    public async Task DeleteUnknownIsNotFound()
    {
        var response = await _client.DeleteAsync("/api/customers/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "Customer API returns a JSON array")]
    public async Task ApiReturnsArray()
    {
        var json = await GetJsonAsync("/api/customers");

        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);
        var first = json.RootElement[0];
        Assert.True(first.TryGetProperty("id", out _));
        Assert.True(first.TryGetProperty("name", out _));
        Assert.True(first.TryGetProperty("membershipType", out var membership));
        Assert.Equal("Monthly", membership.GetProperty("name").GetString());
    }

    [Fact(DisplayName = "Customer API filters by a name fragment")]
    public async Task ApiFiltersByName()
    {
        var john = await GetJsonAsync("/api/customers?query=John");
        var lower = await GetJsonAsync("/api/customers?query=john");

        Assert.Contains("John Smith", john.RootElement.ToString());
        Assert.DoesNotContain("Mary Williams", john.RootElement.ToString());
        Assert.Equal(0, lower.RootElement.GetArrayLength());
    }

    [Fact(DisplayName = "Customer API returns one customer")]
    public async Task ApiReturnsOneCustomer()
    {
        var json = await GetJsonAsync("/api/customers/1");

        Assert.Equal("John Smith", json.RootElement.GetProperty("name").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("id").GetInt32());
    }

    [Fact(DisplayName = "Customer API returns not found for an unknown id")]
    public async Task ApiGetUnknownIsNotFound()
    {
        var response = await _client.GetAsync("/api/customers/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "Customer API creates a customer")]
    public async Task ApiCreatesCustomer()
    {
        var response = await _client.PostAsJsonAsync("/api/customers", new
        {
            name = "Ada Lovelace",
            membershipTypeId = 1,
            isSubscribedToNewsletter = false
        });
        var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(json.RootElement.GetProperty("id").GetInt32() > 0);
        Assert.NotNull(_store.Find(json.RootElement.GetProperty("id").GetInt32()));
    }

    [Fact(DisplayName = "Customer API rejects a create with no name")]
    public async Task ApiRejectsCreateWithoutName()
    {
        var response = await _client.PostAsJsonAsync("/api/customers", new
        {
            name = "",
            membershipTypeId = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(2, _store.Count);
    }

    [Fact(DisplayName = "Customer API updates a customer")]
    public async Task ApiUpdatesCustomer()
    {
        var response = await _client.PutAsJsonAsync("/api/customers/1", new
        {
            name = "Jane Smith",
            membershipTypeId = 2,
            isSubscribedToNewsletter = true,
            birthdate = "1981-02-02"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Jane Smith", _store.Find(1)!.Name);
    }

    [Fact(DisplayName = "Customer API returns not found when updating an unknown id")]
    public async Task ApiUpdateUnknownIsNotFound()
    {
        var response = await _client.PutAsJsonAsync("/api/customers/999", new
        {
            name = "Nobody",
            membershipTypeId = 1
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "Customer API rejects an update with no name")]
    public async Task ApiRejectsUpdateWithoutName()
    {
        var response = await _client.PutAsJsonAsync("/api/customers/1", new
        {
            name = "",
            membershipTypeId = 2
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("John Smith", _store.Find(1)!.Name);
    }

    [Fact(DisplayName = "Customer API does not apply the 18-year membership rule")]
    public async Task ApiSkipsAgeRule()
    {
        var response = await _client.PostAsJsonAsync("/api/customers", new
        {
            name = "Young Member",
            membershipTypeId = 2,
            birthdate = $"{DateTime.Today.Year - 17}-06-15"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(3, _store.Count);
    }

    [Fact(DisplayName = "Customer pages show the Vidly menu and footer")]
    public async Task CustomerPagesShowChrome()
    {
        var html = await GetHtmlAsync("/Customers/Index");

        Assert.Contains("Vidly", html);
        Assert.Contains("New Rental", html);
        Assert.Contains("Customers", html);
        Assert.Contains("Movies", html);
        Assert.Contains("Register", html);
        Assert.Contains("Log in", html);
        Assert.Contains("My ASP.NET Application", html);
        Assert.Contains("rel=\"stylesheet\"", html);
    }

    private async Task<string> GetHtmlAsync(string url)
    {
        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
    }

    private async Task<JsonDocument> GetJsonAsync(string url)
    {
        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
    }

    private async Task<string> PostSaveHtmlAsync(Dictionary<string, string> fields)
    {
        var response = await PostSaveAsync("/Customers/New", fields);
        return System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
    }

    private async Task<HttpResponseMessage> PostSaveAsync(string pageUrl, Dictionary<string, string> fields)
    {
        var page = await GetHtmlAsync(pageUrl);
        var tag = Regex.Match(page, "<input[^>]*name=\"__RequestVerificationToken\"[^>]*>");
        var match = Regex.Match(tag.Value, "value=\"([^\"]+)\"");
        Assert.True(match.Success);
        fields["__RequestVerificationToken"] = match.Groups[1].Value;
        return await _client.PostAsync("/Customers/Save", new FormUrlEncodedContent(fields));
    }
}
