using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wave.Data;
using Wave.Data.Transactional;
using Wave.Tests.TestUtilities;

namespace Wave.Tests.Data;

[TestFixture, FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[TestOf(typeof(ApplicationRepository))]
public class ApplicationRepositoryTest : DbContextTest {
	private ApplicationRepository Repository { get; set; }
	private const string TestUserName = "testuser@example.com";
	private const string AuthorUserName = "author@example.com";
	private ClaimsPrincipal? AnonymousPrincipal { get; set; }
	private ClaimsPrincipal? UserPrincipal { get; set; }
	private ClaimsPrincipal? AuthorPrincipal { get; set; }

	private class MockDbContextFactory(Func<ApplicationDbContext> supplier) : IDbContextFactory<ApplicationDbContext> {
		public ApplicationDbContext CreateDbContext() => supplier();
	}

	public override async Task SetUp() {
		await base.SetUp();
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
	}

	[Test]
	public void CreateArticleMissingTitle_ThrowsMalformed() {
		var article = new ArticleDto(null, 
			null!, 
			"*Test* Body", 
			null, null, null, null);
		
		Assert.ThrowsAsync<ArticleMalformedException>(
			async () => await Repository.CreateArticle(article, AuthorPrincipal!));
	}
	
	[Test]
	public void CreateArticleRegularUser_ThrowsMissingPermissions() {
		var article = GetValidTestArticle();

		Assert.ThrowsAsync<ArticleMissingPermissionsException>(
			async () => await Repository.CreateArticle(article, UserPrincipal!));
	}

	[Test]
	public void CreateArticleAnonymousUser_ThrowsMissingPermissions() {
		var article = GetValidTestArticle();

		Assert.ThrowsAsync<ArticleMissingPermissionsException>(
			async () => await Repository.CreateArticle(article, AnonymousPrincipal!));
	}

	[Test]
	public async Task CreateArticle() {
		var article = GetValidTestArticle();

		var view = await Repository.CreateArticle(article, AuthorPrincipal!);

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
	public async Task CreateArticleWithCategories() {
		Guid categoryId;
		{
			await using var c = GetContext();
			categoryId = c.Set<Category>().First(cat => cat.Color == CategoryColors.Primary).Id;
		}
		var article = GetValidTestArticle([categoryId]);
		var view = await Repository.CreateArticle(article, AuthorPrincipal!);
		
		await using var context = GetContext();
		Assert.Multiple(() => {
			Assert.That(view.Categories, Has.Count.EqualTo(1));
			Assert.That(context.Set<Article>().IgnoreQueryFilters()
				.Include(a => a.Categories)
				.First(a => a.Id == view.Id).Categories.First().Id, Is.EqualTo(categoryId));
		});
	}

	private static ArticleDto GetValidTestArticle(Guid[]? categories = null) {
		return new ArticleDto(
			null,
			"Test Article",
			"*Test* Body",
			null, null, categories, null);
	}
}