using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using wave.server.Data;
using wave.server.Endpoints.Article;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var api = app.MapGroup("api");

api.MapArticleEndpoints();

app.Run();
