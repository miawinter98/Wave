using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tomlyn.Extensions.Configuration;
using Wave.Components;
using Wave.Components.Account;
using Wave.Data;
using Wave.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("/configuration/config.json", true, false)
    .AddYamlFile("/configuration/config.yml", true, false)
    .AddTomlFile("/configuration/config.toml", true, false)
    .AddIniFile( "/configuration/config.ini", true, false)
    .AddXmlFile( "/configuration/config.xml", true, false)
    .AddEnvironmentVariables("WAVE_");

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddControllers();

#region Authentication & Authorization

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

// Authors: Can create Articles, require them to be reviewed
// Reviewers: Can review Articles, but cannot create them themselves
// Moderators: Can delete Articles / take them Offline
// Admins: Can do anything, and assign roles to other users
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ArticleEditPermissions", p => p.RequireRole("Author", "Admin"))
    .AddPolicy("ArticleReviewPermissions", p => p.RequireRole("Reviewer", "Admin"))
    .AddPolicy("ArticleDeletePermissions", p => p.RequireRole("Moderator", "Admin"))
    .AddPolicy("RoleAssignPermissions", p => p.RequireRole("Admin"));
builder.Services.AddAuthentication(options => {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    }).AddIdentityCookies();

#endregion

#region Identity

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                          ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<UserClaimsFactory>();

#endregion

#region Services

builder.Services.AddLocalization(options => {
    options.ResourcesPath = "Resources";
});
builder.Services.AddScoped<ImageService>();

builder.Services.Configure<Customization>(builder.Configuration.GetSection(nameof(Customization)));
builder.Services.AddCascadingValue("TitlePrefix", 
    sf => (sf.GetService<IOptions<Customization>>()?.Value.AppName ?? "Wave") + " - ");

var smtpConfig = builder.Configuration.GetSection("Email:Smtp");
if (smtpConfig.Exists()) {
    builder.Services.Configure<SmtpConfiguration>(smtpConfig);
    builder.Services.AddScoped<IEmailSender<ApplicationUser>, SmtpEmailSender>();
} else {
    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
}

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseMigrationsEndPoint();
} else {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapControllers();

string[] cultures = ["en-US", "en-GB", "de-DE"];
app.UseRequestLocalization(new RequestLocalizationOptions {
        ApplyCurrentCultureToResponseHeaders = true,
        FallBackToParentCultures = true, FallBackToParentUICultures = true
    }
    .SetDefaultCulture(cultures[0])
    .AddSupportedCultures(cultures)
    .AddSupportedUICultures(cultures));

{
    using var scope = app.Services.CreateScope();
    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    
    if (userManager.GetUsersInRoleAsync("Admin").Result.Any() is false) {
        string admin = Guid.NewGuid().ToString("N")[..16];
        Console.WriteLine(
            "There is currently no user in your installation with the admin role, " +
            "go to /Admin and use the following password to self promote your account: " + admin);
        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "admin.txt"), admin);
    }
}

app.Run();
