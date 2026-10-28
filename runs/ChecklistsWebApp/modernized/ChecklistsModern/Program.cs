using ChecklistsModern.Data;
using ChecklistsModern.Filters;
using ChecklistsModern.Identity;
using ChecklistsModern.Models;
using ChecklistsModern.Options;
using ChecklistsModern.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.Configure<SmsOptions>(builder.Configuration.GetSection(SmsOptions.SectionName));
builder.Services.Configure<ExternalAuthenticationOptions>(builder.Configuration.GetSection(ExternalAuthenticationOptions.SectionName));

builder.Services.AddDbContext<ApplicationDbContext>((services, options) =>
{
    var databaseOptions = services.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    var connectionString = services.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not set.");

    if (string.Equals(databaseOptions.Provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
        options.UseSqlServer(connectionString);
    else
        options.UseSqlite(connectionString);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.AllowedUserNameCharacters = string.Empty;
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Tokens.ChangePhoneNumberTokenProvider = "Phone Code";
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddErrorDescriber<LegacyIdentityErrorDescriber>()
    .AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>()
    .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider)
    .AddTokenProvider<PhoneNumberTokenProvider<ApplicationUser>>("Phone Code")
    .AddTokenProvider<EmailTokenProvider<ApplicationUser>>("Email Code");

builder.Services.RemoveAll<IPasswordValidator<ApplicationUser>>();
builder.Services.AddScoped<IPasswordValidator<ApplicationUser>, LegacyPasswordValidator>();

builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.FromMinutes(30));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    var redirectToLogin = options.Events.OnRedirectToLogin;
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        return redirectToLogin(context);
    };
});

builder.Services.AddScoped<IChecklistService, ChecklistService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddSingleton<IEmailSender, NoOpEmailSender>();
builder.Services.AddSingleton<ISmsSender, NoOpSmsSender>();

builder.Services.AddControllersWithViews(options =>
{
    if (!builder.Environment.IsDevelopment())
        options.Filters.Add<HandleErrorFilter>();
});
builder.Services.Replace(ServiceDescriptor.Singleton<IHtmlGenerator, LegacyHtmlGenerator>());

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var options = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    if (options.EnsureCreated)
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();
    if (options.SeedPreviewData)
        await PreviewSeeder.SeedAsync(scope.ServiceProvider);

    var external = scope.ServiceProvider.GetRequiredService<IOptions<ExternalAuthenticationOptions>>().Value;
    if (new[] { external.Google, external.Microsoft, external.Facebook, external.Twitter }.Any(p => !string.IsNullOrEmpty(p.ClientId)))
        app.Logger.LogWarning("An external login ClientId is set, but no external provider is registered in this build.");
}

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

// MVC 5 answered a wrong HTTP verb on an MVC action with 404; Web API 2 answered 405.
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == StatusCodes.Status405MethodNotAllowed
        && !context.Request.Path.StartsWithSegments("/api")
        && !context.Response.HasStarted)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
    }
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Checklists}/{action=Index}/{id?}");

app.Run();

public partial class Program;
