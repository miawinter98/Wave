using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Wave.Data;

namespace Wave.Tests.TestUtilities;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public abstract class DbContextTest {
	private PostgreSqlContainer PostgresContainer { get; } = new PostgreSqlBuilder().WithImage("postgres:16.1-alpine").Build();

	[SetUp]
	public async Task SetUp() {
		await PostgresContainer.StartAsync();
		await AndThenSetUp();
	}
	protected virtual ValueTask AndThenSetUp() {
		return ValueTask.CompletedTask;
	}

	[TearDown]
	public async Task TearDown() => await PostgresContainer.DisposeAsync();

	protected ApplicationDbContext GetContext() =>
		new(new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseNpgsql(PostgresContainer.GetConnectionString())
				.EnableSensitiveDataLogging()
				.EnableDetailedErrors()
				.EnableThreadSafetyChecks()
				.Options);

}