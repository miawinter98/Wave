using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using wave.server.Data;

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

var summaries = new[]
{
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching",
};

app.MapGet(
        "/weatherforecast",
        () =>
        {
            var forecast = Enumerable
                .Range(1, 5)
                .Select(index => new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        }
    )
    .WithName("GetWeatherForecast");

app.Map(
    "/test",
    async Task<Results<Ok<ArticleRecord>, NotFound>> (
        IDbContextFactory<DatabaseContext> context,
        CancellationToken cancellation
    ) =>
    {
        await using var dao = await context.CreateDbContextAsync(cancellation);
        var article = await dao.Set<ArticleRecord>().FirstOrDefaultAsync(cancellation);

        return article is null ? TypedResults.NotFound() : TypedResults.Ok(article);
    }
);

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
