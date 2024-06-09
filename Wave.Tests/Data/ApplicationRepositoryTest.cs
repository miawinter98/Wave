using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wave.Data;
using Wave.Data.Transactional;
using Wave.Tests.TestUtilities;
// ReSharper disable InconsistentNaming

namespace Wave.Tests.Data;

public abstract class ApplicationRepositoryTests: DbContextTest {
	protected ApplicationRepository Repository { get; set; } = null!;
	
	protected const string TestUserName = "testuser@example.com";
	protected const string AuthorUserName = "author@example.com";
	
	protected ClaimsPrincipal AnonymousPrincipal { get; set; } = null!;
	protected ClaimsPrincipal UserPrincipal { get; set; } = null!;
	protected ClaimsPrincipal AuthorPrincipal { get; set; } = null!;
	
	protected Guid PrimaryCategoryId { get; set; }
	protected Guid SecondaryCategoryId { get; set; }

	protected virtual ValueTask InitializeTestEntities(ApplicationDbContext context) {
		return ValueTask.CompletedTask;
	}

	protected override async ValueTask AndThenSetUp() {
		Repository = new ApplicationRepository(new MockDbContextFactory(GetContext));
		
		List<Category> categories = [
			new Category {
				Name = "Primary Category",
				Color = CategoryColors.Primary
			},
			new Category() {
				Name = "Secondary Category",
				Color = CategoryColors.Secondary
			}
		];

		await using var context = GetContext();
		var user = new ApplicationUser() {
			UserName = TestUserName
		};
		var author = new ApplicationUser() {
			UserName = AuthorUserName
		};
		
		context.AddRange(categories);
		context.Users.AddRange([user, author]);

		await context.Database.EnsureCreatedAsync();
		await context.SaveChangesAsync();

		AnonymousPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
		UserPrincipal = new ClaimsPrincipal(new ClaimsIdentity([
			new Claim(ClaimTypes.Name, user.UserName),
			new Claim(ClaimTypes.NameIdentifier, user.Id),
			new Claim("Id", user.Id),
		], "Mock Authentication"));
		AuthorPrincipal = new ClaimsPrincipal(new ClaimsIdentity([
			new Claim(ClaimTypes.Name, author.UserName),
			new Claim(ClaimTypes.NameIdentifier, author.Id),
			new Claim("Id", author.Id),
			new Claim(ClaimTypes.Role, "Author"),
		], "Mock Authentication"));
		PrimaryCategoryId = categories[0].Id;
		SecondaryCategoryId = categories[1].Id;

		await InitializeTestEntities(context);
	}
	
	private sealed class MockDbContextFactory(Func<ApplicationDbContext> supplier) : IDbContextFactory<ApplicationDbContext> {
		public ApplicationDbContext CreateDbContext() => supplier();
	}
}

[TestFixture, FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[TestOf(typeof(ApplicationRepository))]
public class ApplicationRepositoryTest_CreateArticle : ApplicationRepositoryTests {
	private static ArticleDto GetValidTestArticle() {
		return new ArticleDto(
			null,
			"Test Article",
			"*Test* Body",
			null, null, null, null);
	}

	#region Success Tests
	
	[Test]
	public async Task MinimalArticle_Success() {
		var article = GetValidTestArticle();

		var view = await Repository.CreateArticle(article, AuthorPrincipal);

		await using var context = GetContext();
		Assert.Multiple(() => {
			Assert.That(context.Set<Article>().IgnoreQueryFilters().ToList(), Has.Count.EqualTo(1));
			Assert.That(context.Set<Article>().IgnoreQueryFilters().First().Id, Is.EqualTo(view.Id));
			Assert.That(view.Status, Is.EqualTo(ArticleStatus.Draft));
			Assert.That(view.BodyHtml, Is.Not.Null);
			Assert.That(view.BodyPlain, Is.Not.Null);
			Assert.That(view.Slug, Is.EqualTo("test-article"));
		});
	}
	[Test]
	public async Task WithCategories_Success() {
		var article = GetValidTestArticle() with {Categories = [PrimaryCategoryId]};
		var view = await Repository.CreateArticle(article, AuthorPrincipal);
		
		await using var context = GetContext();
		Assert.Multiple(() => {
			Assert.That(view.Categories, Has.Count.EqualTo(1));
			Assert.That(context.Set<Article>().IgnoreQueryFilters()
				.Include(a => a.Categories)
				.First(a => a.Id == view.Id).Categories.First().Id, Is.EqualTo(PrimaryCategoryId));
		});
	}

	#endregion

	#region Permission Tests

	[Test]
	public void RegularUser_ThrowsMissingPermissions() {
		var article = GetValidTestArticle();

		Assert.ThrowsAsync<ArticleMissingPermissionsException>(
			async () => await Repository.CreateArticle(article, UserPrincipal));
	}

	[Test]
	public void AnonymousUser_ThrowsMissingPermissions() {
		var article = GetValidTestArticle();

		Assert.ThrowsAsync<ArticleMissingPermissionsException>(
			async () => await Repository.CreateArticle(article, AnonymousPrincipal));
	}

	#endregion

	#region Data Validation Tests
	
	[Test]
	public void MissingTitle_ThrowsMalformed() {
		var article = new ArticleDto(null, 
			null!, 
			"*Test* Body", 
			null, null, null, null);
		
		Assert.ThrowsAsync<ArticleMalformedException>(
			async () => await Repository.CreateArticle(article, AuthorPrincipal));
	}
	

	[Test]
	public void WithIdNotNull_ThrowsArticleException() {
		var article = GetValidTestArticle() with { Id = new Guid() };

		Assert.ThrowsAsync<ArticleException>(
			async () => await Repository.CreateArticle(article, AuthorPrincipal));
	}

	#endregion
}

[TestFixture, FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[TestOf(typeof(ApplicationRepository))]
public class ApplicationRepositoryTest_UpdateArticle : ApplicationRepositoryTests {
	private Guid TestArticleId { get; set; }

	protected override async ValueTask InitializeTestEntities(ApplicationDbContext context) {
		var testArticle = new ArticleDto(null,
			"Test Article", 
			"Test **Article** with *formatting.", 
			"test-article", 
			DateTimeOffset.Now.AddHours(-5),
			[PrimaryCategoryId], null);

		var view = await Repository.CreateArticle(testArticle, AuthorPrincipal);
		TestArticleId = view.Id;
	}


}