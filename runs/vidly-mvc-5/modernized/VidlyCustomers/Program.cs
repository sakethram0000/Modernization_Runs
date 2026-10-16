using System.Globalization;
using VidlyCustomers.Models;
using VidlyCustomers.Services;

var culture = CultureInfo.GetCultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        value => $"The value '{value}' is invalid.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (value, _) => $"The value '{value}' is invalid.");
});
builder.Services.Configure<MembershipCatalog>(builder.Configuration.GetSection("MembershipCatalog"));
builder.Services.AddSingleton<CustomerStore>();
builder.Services.AddScoped<CustomerService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program;
