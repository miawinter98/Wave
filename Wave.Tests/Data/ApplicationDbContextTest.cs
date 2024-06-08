using Microsoft.EntityFrameworkCore;
using Wave.Data;
using Wave.Tests.TestUtilities;

namespace Wave.Tests.Data;

[TestFixture, FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[TestOf(typeof(ApplicationDbContext))]
public class ApplicationDbContextTest : DbContextTest {
	[Test]
	public async Task Migration() {
		await using var context = GetContext();
		Assert.DoesNotThrowAsync(() => context.Database.MigrateAsync());
	}

	[Test]
	public async Task CreateArticle() {
		await using var context = GetContext();
		await context.Database.EnsureCreatedAsync();

		var author = new ApplicationUser {
			FullName = "Test User"
		};
		Article article = new() {
			Title = "Testing Article",
			Body = "This is a *test* Article",
			Author = author
		};
		article.UpdateSlug(null);
		article.UpdateBody();

		await context.AddAsync(article);
		Assert.DoesNotThrowAsync(() => context.SaveChangesAsync());

		var dbArticle = await context.Set<Article>()
			.IgnoreQueryFilters().FirstOrDefaultAsync();
		Assert.That(dbArticle, Is.Not.Null);

		Assert.That(dbArticle.Title, Is.EqualTo("Testing Article"));
		Assert.That(dbArticle.Slug, Is.EqualTo("testing-article"));

		Assert.That(dbArticle.Body, Is.EqualTo("This is a *test* Article"));
		Assert.That(dbArticle.BodyPlain, Is.EqualTo("This is a test Article"));
		Assert.That(dbArticle.BodyHtml, Is.EqualTo("<p>This is a <em>test</em> Article</p>"));
	}
}