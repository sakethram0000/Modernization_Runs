using System.Net;
using System.Text.RegularExpressions;

namespace ChecklistsModern.Tests.Infrastructure;

// An HttpClient with its own cookie jar, used like a browser: no automatic redirects.
public sealed partial class Browser
{
    private readonly HttpClient _client;

    public Browser(HttpClient client)
    {
        _client = client;
    }

    public Task<HttpResponseMessage> GetAsync(string url) => _client.GetAsync(url);

    public async Task<string> GetHtmlAsync(string url)
    {
        var response = await _client.GetAsync(url);
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"GET {url} returned {(int)response.StatusCode} {response.Headers.Location}");
        return await response.HtmlAsync();
    }

    public async Task<string> TokenAsync(string page = "/Account/Login")
    {
        var response = await _client.GetAsync(page);
        var html = await response.Content.ReadAsStringAsync();
        var match = TokenPattern().Match(html);
        Assert.True(match.Success, $"No antiforgery token on {page} ({(int)response.StatusCode})");
        return match.Groups[1].Value;
    }

    public async Task<HttpResponseMessage> PostFormAsync(string url, IDictionary<string, string?> fields, string tokenPage = "/Account/Login")
    {
        var token = await TokenAsync(tokenPage);
        var body = new Dictionary<string, string?>(fields) { ["__RequestVerificationToken"] = token };
        return await PostFormWithoutTokenAsync(url, body);
    }

    public Task<HttpResponseMessage> PostFormWithoutTokenAsync(string url, IDictionary<string, string?> fields) =>
        _client.PostAsync(url, new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Key, f.Value ?? string.Empty))));

    public async Task<HttpResponseMessage> SignInAsync(string email, string password = TestApp.Password, string? returnUrl = "/")
    {
        var url = returnUrl == null ? "/Account/Login" : $"/Account/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}";
        var response = await PostFormAsync(url, new Dictionary<string, string?> { ["Email"] = email, ["Password"] = password, ["RememberMe"] = "false" });
        return response;
    }

    public Task<HttpResponseMessage> AjaxAsync(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        return _client.SendAsync(request);
    }

    public Task<HttpResponseMessage> SendAsync(HttpMethod method, string url) =>
        _client.SendAsync(new HttpRequestMessage(method, url));

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenPattern();
}

public static class ResponseExtensions
{
    public static async Task<string> HtmlAsync(this HttpResponseMessage response) =>
        WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

    // Path and query of the Location header, whether the app sent it absolute or relative.
    public static string Location(this HttpResponseMessage response)
    {
        var location = response.Headers.Location;
        Assert.True(location != null, $"Expected a redirect but got {(int)response.StatusCode}");
        return location!.IsAbsoluteUri ? location.PathAndQuery : location.OriginalString;
    }

    public static void AssertRedirect(this HttpResponseMessage response, string expected)
    {
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal(expected, response.Location());
    }

    public static void AssertRedirectToLogin(this HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Account/Login", response.Location());
    }
}
