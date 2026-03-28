using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using wave.server.Configuration;
using wave.server.Data;
using wave.server.Endpoints.Article;
using wave.server.Endpoints.User;
using wave.server.Modules.ActivityPub;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables("Wave_");
builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options =>
{
	options.SerializerOptions.AllowOutOfOrderMetadataProperties = true;
	options.SerializerOptions.AllowTrailingCommas = true;
	options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#region Configuration

builder.Services.Configure<ActivityPubOptions>(builder.Configuration.GetSection("ActivityPub"));

#endregion

#region Data

var dataConnection =
	builder.Configuration.GetConnectionString("postgres")
	?? throw new InvalidOperationException("Connection string 'postgres' not found.");
builder.Services.AddDbContextFactory<DatabaseContext>(options =>
{
	options.UseNpgsql(dataConnection);
#if DEBUG
	options.EnableDetailedErrors();
	options.EnableSensitiveDataLogging();
	// TODO LogTo Serilog Logger
#endif
});

#endregion

#region Identity

builder.Services.AddAuthorizationBuilder();

builder
	.Services.AddIdentityApiEndpoints<ApplicationUser>()
	.AddEntityFrameworkStores<DatabaseContext>();

#endregion

#region Services

var services = builder.Services;

services.AddScoped<ArticleRepository>();
services.AddScoped<ArticleService>();

services.AddActivityPub();

#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.UseReDoc(options =>
	{
		options.SpecUrl = "/openapi/v1.json";
		options.RoutePrefix = "documentation";
		options.DocumentTitle = "Wave API";
	});
}

var ap = app.Services.GetService<IOptions<ActivityPubOptions>>();
if (ap?.Value is not null && !string.IsNullOrEmpty(ap.Value.Domain))
{
	Console.WriteLine("Launching ActivityPub Integration on domain " + ap.Value.Domain); // TODO log
	app.MapActivityPubEndpoints();
}

var api = app.MapGroup("api");

// app.MapIdentityApi<ApplicationUser>();
api.MapUserEndpoints();
api.MapArticleEndpoints();

app.Run();
