using System.Reflection;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text;
using AspNetCore.Authentication.ApiKey;
using Tomlyn.Extensions.Configuration;
using Wave.Components;
using Wave.Components.Account;
using Wave.Data;
using Wave.Services;
using Wave.Utilities;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;
using Vite.AspNetCore;
using Wave.Utilities.Metrics;

#region Version Information

string humanReadableVersion = Assembly.GetEntryAssembly()?
	.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
	.InformationalVersion.Split("+", 2)[0] ?? "unknown";

#endregion

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.CreateBootstrapLogger();
var logger = Log.Logger.ForContext<Program>();
logger.Information("Starting Wave {WaveVersion}", humanReadableVersion);
builder.Services.AddCascadingValue("Version", _ => humanReadableVersion);

builder.Configuration
	.AddJsonFile(Path.Combine(FileSystemService.ConfigurationDirectory, "config.json"), true, false)
	.AddYamlFile(Path.Combine(FileSystemService.ConfigurationDirectory, "config.yml"), true, false)
	.AddTomlFile(Path.Combine(FileSystemService.ConfigurationDirectory, "config.toml"), true, false)
	.AddIniFile( Path.Combine(FileSystemService.ConfigurationDirectory, "config.ini"), true, false)
	.AddXmlFile( Path.Combine(FileSystemService.ConfigurationDirectory, "config.xml"), true, false)
	.AddEnvironmentVariables("WAVE_");

var customizations = builder.Configuration.GetSection(nameof(Customization)).Get<Customization>();

#region Logging

builder.Services.AddSerilog((services, configuration) => {
	configuration
		.MinimumLevel.Verbose()
		.ReadFrom.Services(services)
		.Enrich.WithProperty("Application", "Wave")
		.Enrich.WithProperty("WaveVersion", humanReadableVersion)
		.Enrich.WithProperty("AppName", customizations?.AppName)
		.Enrich.FromLogContext();
	if (builder.Configuration["loki"] is {} lokiConfiguration) { 
		configuration.WriteTo.GrafanaLoki(lokiConfiguration, null, [
			"Application", "WaveVersion", "AppName", "level", "SourceContext", "RequestId", "RequestPath"
		], restrictedToMinimumLevel:LogEventLevel.Debug);
	} else {
		configuration.WriteTo.Console(restrictedToMinimumLevel:LogEventLevel.Information);
	}
});

#endregion

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddControllers(options => {
	options.OutputFormatters.Add(new SyndicationFeedFormatter());
});
builder.Services.AddOutputCache();
builder.Services.AddViteServices(options => {
	// options.Server.AutoRun = true;
	options.Server.ScriptName = "dev";
	options.Server.Https = false;
	options.Server.UseReactRefresh = true;
	options.Base = "/dist/";
});

#region Data Protection & Redis

if (builder.Configuration.GetConnectionString("Redis") is { } redisUri) {
	var redis = ConnectionMultiplexer.Connect(redisUri);
	builder.Services.AddDataProtection()
		.PersistKeysToStackExchangeRedis(redis)
		.UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration() {
			EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
			ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
		});
	builder.Services.AddStackExchangeRedisCache(options => {
		options.Configuration = redisUri;
		options.InstanceName = "WaveDistributedCache";
	});
	builder.Services.AddStackExchangeRedisOutputCache(options => {
		options.Configuration = redisUri;
		options.InstanceName = "WaveOutputCache";
	});
} else {
	builder.Services.AddDataProtection()
		.UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration {
			EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
			ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
		});
	builder.Services.AddDistributedMemoryCache();
	logger.Warning("No Redis connection string found, running in-memory.");
}

#endregion

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
	.AddPolicy("CategoryManagePermissions", p => p.RequireRole("Admin"))
	.AddPolicy("RoleAssignPermissions", p => p.RequireRole("Admin"))

	.AddPolicy("ArticleEditOrReviewPermissions", p => p.RequireRole("Author", "Reviewer", "Admin"))
	
	.AddPolicy("EmailApi", p => p.RequireClaim("EmailApi")
	.AddAuthenticationSchemes(ApiKeyDefaults.AuthenticationScheme));
builder.Services.AddAuthentication(options => {
		options.DefaultScheme = IdentityConstants.ApplicationScheme;
		options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
	}).AddApiKeyInHeader<ApiKeyProvider>(ApiKeyDefaults.AuthenticationScheme, options => {
		options.KeyName = "X-API-KEY";
		options.Realm = "Wave API";
	}).AddApiKeyInRouteValues<ApiKeyProvider>("ApiKeyInRoute", options => {
		options.KeyName = "apiKey";
		options.Realm = "Wave API";
	})
	.AddIdentityCookies();
if (builder.Configuration.GetSection("Oidc").Get<OidcConfiguration>() is {} oidc && !string.IsNullOrWhiteSpace(oidc.Authority)) {
	builder.Services.AddAuthentication(options => {
		options.DefaultScheme = IdentityConstants.ApplicationScheme;
		options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
	}).AddOpenIdConnect(options => {
		options.SignInScheme = IdentityConstants.ExternalScheme;

		options.Scope.Add(OpenIdConnectScope.OpenIdProfile);
		options.Scope.Add(OpenIdConnectScope.OfflineAccess);
		options.Authority = oidc.Authority;

		options.ClientId = oidc.ClientId;
		options.ClientSecret = oidc.ClientSecret;
		options.ResponseType = OpenIdConnectResponseType.Code;

		options.MapInboundClaims = false;
		options.TokenValidationParameters.NameClaimType = "name";
		options.TokenValidationParameters.RoleClaimType = "role";

		options.CallbackPath = new PathString("/signin-oidc");
		options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
		options.RemoteSignOutPath = new PathString("/signout-oidc");

		options.Events.OnRedirectToIdentityProvider = context => {
			var uri = new UriBuilder(context.ProtocolMessage.RedirectUri) {
				Scheme = "https",
				Port = -1
			};
			context.ProtocolMessage.RedirectUri = uri.ToString();
			return Task.FromResult(0);
		};
	});
}

#endregion

#region Identity

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
						  ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
	options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => {
		options.SignIn.RequireConfirmedAccount = true;
		options.ClaimsIdentity.UserIdClaimType = "Id";
	})
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddSignInManager()
	.AddDefaultTokenProviders()
	.AddClaimsPrincipalFactory<UserClaimsFactory>();
builder.Services.AddScoped<ApplicationRepository>();

#endregion

#region Services

builder.Services.AddHealthChecks();
builder.Services.AddLocalization(options => {
	options.ResourcesPath = "Resources";
});
builder.Services.AddScoped<ImageService>();
builder.Services.AddHttpClient();

builder.Services.Configure<Features>(builder.Configuration.GetSection(nameof(Features)));
builder.Services.Configure<Customization>(builder.Configuration.GetSection(nameof(Customization)));
builder.Services.AddCascadingValue("TitlePostfix", sf => " | " + (sf.GetService<IOptions<Customization>>()?.Value.AppName ?? "Wave"));

var emailConfig = builder.Configuration.GetSection("Email").Get<EmailConfiguration>();
builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("Email"));
builder.Services.AddSingleton<EmailTemplateService>();
builder.Services.AddScoped<EmailFactory>();
if (emailConfig?.Smtp.Count > 0) {
	if (string.IsNullOrWhiteSpace(emailConfig.SenderEmail)) {
		throw new ApplicationException(
			"Email providers have been configured, but no SenderEmail. " +
			"Please provider the sender email address used for email distribution.");
	}

	foreach (var smtp in emailConfig.Smtp) {
		builder.Services.AddKeyedScoped<IEmailService, SmtpEmailService>(smtp.Key.ToLower(), (provider, key) => 
			ActivatorUtilities.CreateInstance<SmtpEmailService>(provider, 
				provider.GetRequiredService<IOptions<EmailConfiguration>>().Value.Smtp[(string)key]));
	}

	if (emailConfig.Smtp.Keys.Any(k => k.Equals("live", StringComparison.CurrentCultureIgnoreCase))) {
		builder.Services.AddScoped(sp => sp.GetKeyedService<IEmailService>("live")!);
		builder.Services.AddScoped<IEmailSender<ApplicationUser>, IdentityEmailSender>();
	} else {
		builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
		logger.Warning("No 'live' email provider configured.");
	}

	if (emailConfig.Smtp.Keys.Any(k => k.Equals("bulk", StringComparison.CurrentCultureIgnoreCase))) {
		builder.Services.AddScoped<NewsletterBackgroundService>();
		builder.Services.AddHostedService<EmailBackgroundWorker>();
	} else if (builder.Configuration.GetSection(nameof(Features)).Get<Features>()?.EmailSubscriptions is true) {
		throw new ApplicationException(
			"Email subscriptions have been enabled, but no 'bulk' email provider was configured. " + 
			"Disable email subscriptions or provide the mail provider for bulk sending");
	}
} else {
	builder.Services.AddSingleton<IEmailService, NoOpEmailService>();
	builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
	logger.Warning("No email provider configured.");
}

builder.Services.AddScoped<IMessageDisplay, MessageService>();
builder.Services.AddSingleton<FileSystemService>();


#endregion

#region Localization

var customization = builder.Configuration.GetSection(nameof(Customization)).Get<Customization>();
string[] cultures = ["en-US", "en-GB", "de-DE"];

string defaultLanguage = string.IsNullOrWhiteSpace(customization?.DefaultLanguage) ? cultures[0] : customization.DefaultLanguage;
builder.Services.Configure<RequestLocalizationOptions>(options => {
	options.ApplyCurrentCultureToResponseHeaders = true;
	options.FallBackToParentCultures = true;
	options.FallBackToParentUICultures = true;
	options.SetDefaultCulture(defaultLanguage)
		.AddSupportedCultures(cultures)
		.AddSupportedUICultures(cultures);
});

#endregion

#region Open Telemetry & Metrics

var features = builder.Configuration.GetSection(nameof(Features)).Get<Features>();
if (features?.Telemetry is true) {
	var otel = builder.Services.AddOpenTelemetry();

	otel.ConfigureResource(resource => resource.AddService(serviceName:customization?.AppName ?? "Wave"));

	// Prometheus
	otel.WithMetrics(metrics => metrics
		.AddAspNetCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddMeter("Microsoft.AspNetCore.Hosting")
		.AddMeter("Microsoft.AspNetCore.Server.Kestrel")
		.AddMeter("Microsoft.AspNetCore.Http.Connections")
		.AddMeter("Microsoft.AspNetCore.Http.Routing")
		.AddMeter("Microsoft.AspNetCore.Diagnostics")
		.AddMeter("Wave.Api")
		.AddMeter("Wave.Rss")
		.AddPrometheusExporter());
	
	// Jaeger etc.
	if (builder.Configuration["OTLP_ENDPOINT_URL"] is {} otlpUrl) {
		otel.WithTracing(tracing => {
			tracing.AddAspNetCoreInstrumentation(i => i.Filter = context => !string.Equals(context.Request.Path.Value, "/health", StringComparison.InvariantCultureIgnoreCase));
			tracing.AddHttpClientInstrumentation();
			tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpUrl));
		});
	}

	builder.Services.AddSingleton<RssMetrics>();
	builder.Services.AddSingleton<ApiMetrics>();
}

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
	app.UseMigrationsEndPoint();
} else {
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

if (features?.Telemetry is true) {
	app.UseOpenTelemetryPrometheusScrapingEndpoint();
}

app.UseSerilogRequestLogging();

app.UseStaticFiles(new StaticFileOptions {
	ContentTypeProvider = new FileExtensionContentTypeProvider {
		Mappings = {
			[".jxl"] = "image/jxl"
		}
	}
});
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapHealthChecks("/health");
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapAdditionalIdentityEndpoints();
app.MapControllers();

if (app.Environment.IsDevelopment()) {
	//app.UseWebSockets();
	app.UseViteDevelopmentServer(true);
}

app.UseOutputCache();
app.UseRequestLocalization();

{
	using var scope = app.Services.CreateScope();
	await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	context.Database.Migrate();

	var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
	if (userManager.GetUsersInRoleAsync("Admin").Result.Any() is false) {
		var cache = app.Services.GetRequiredService<IDistributedCache>();

		// Check first whether the password exists already
		string? admin = await cache.GetStringAsync("admin_promote_key");

		// If it does not exist, create a new one and save it to redis
		if (string.IsNullOrWhiteSpace(admin)){
			admin = Guid.NewGuid().ToString("N")[..16];
			await cache.SetAsync("admin_promote_key", Encoding.UTF8.GetBytes(admin), new DistributedCacheEntryOptions());
		}

		app.Logger.LogWarning("There is currently no user in your installation with the admin role, " +
							  "go to /Admin and use the following password to self promote your account: {admin}", admin);
	}

	// Generate plain text for Articles created before 1.0.0-alpha.3
	var oldArticles = await context.Set<Article>().IgnoreQueryFilters().IgnoreAutoIncludes()
		.Where(a => a.BodyPlain.Length < 1).AsNoTracking().ToListAsync();
	if (oldArticles.Count > 0) {
		oldArticles.ForEach(a => a.BodyPlain = HtmlUtilities.GetPlainText(a.BodyHtml));
		context.UpdateRange(oldArticles);
		await context.SaveChangesAsync();
	}
}

app.Run();
