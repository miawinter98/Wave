using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Wave.Data;
using Wave.Data.Transactional;
using Wave.Tests.TestUtilities;
// ReSharper disable InconsistentNaming

namespace Wave.Tests.Data;

public abstract class ApplicationRepositoryTests: DbContextTest {
	protected ApplicationRepository Repository { get; set; } = null!;
	
	protected const string TestUserName = "testuser@example.com";
	protected const string AuthorUserName = "author@example.com";
	protected const string ReviewerUserName = "reviewer@example.com";
	
	protected ClaimsPrincipal AnonymousPrincipal { get; set; } = null!;
	protected ClaimsPrincipal UserPrincipal { get; set; } = null!;
	protected ClaimsPrincipal AuthorPrincipal { get; set; } = null!;
	protected ClaimsPrincipal ReviewerPrincipal { get; set; } = null!;
	
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
		var reviewer = new ApplicationUser() {
			UserName = ReviewerUserName
		};
		
		context.AddRange(categories);
		context.Users.AddRange([user, author, reviewer]);

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
		ReviewerPrincipal = new ClaimsPrincipal(new ClaimsIdentity([
			new Claim(ClaimTypes.Name, reviewer.UserName),
			new Claim(ClaimTypes.NameIdentifier, reviewer.Id),
			new Claim("Id", reviewer.Id),
			new Claim(ClaimTypes.Role, "Author"),
			new Claim(ClaimTypes.Role, "Reviewer"),
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
	private static ArticleCreateDto GetValidTestArticle() {
		return new ArticleCreateDto(
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
		var article = GetValidTestArticle() with { Title = null! };
		
		Assert.ThrowsAsync<ArticleMalformedException>(
			async () => await Repository.CreateArticle(article, AuthorPrincipal));
	}

	#endregion
}

[TestFixture, FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[TestOf(typeof(ApplicationRepository))]
public class ApplicationRepositoryTest_UpdateArticle : ApplicationRepositoryTests {
	private Guid TestArticleId { get; set; }

	private ArticleUpdateDto GetValidTestArticle() => new(TestArticleId);

	private static string StringOfLength(int length) {
		var builder = new StringBuilder();
		for (int i = 0; i < length; i++) {
			builder.Append('_');
		}
		return builder.ToString();
	}

	protected override async ValueTask InitializeTestEntities(ApplicationDbContext context) {
		var testArticle = new ArticleCreateDto(
			"Test Article", 
			"Test **Article** with *formatting.", 
			"test-article", 
			DateTimeOffset.Now.AddHours(-5),
			[PrimaryCategoryId], null);

		var view = await Repository.CreateArticle(testArticle, AuthorPrincipal);
		TestArticleId = view.Id;
	}

	#region Success Tests

	[Test]
	public async Task UpdateTitle_Success() {
		var update = GetValidTestArticle() with { Title = "New Title" };

		await Repository.UpdateArticle(update, AuthorPrincipal);

		await using var context = GetContext();
		Assert.Multiple(() => {
			Assert.That(context.Set<Article>().IgnoreQueryFilters().First(a => a.Id == TestArticleId).Title, Is.EqualTo("New Title"));
		});
	}

	[Test]
	public async Task UpdateBodyUpdatesHtmlAndPlain_Success() {
		var update = GetValidTestArticle() with { Body = "Some *new* Body" };
		const string expectedHtml = "<p>Some <em>new</em> Body</p>";
		const string expectedPlain = "Some new Body";
		
		await Repository.UpdateArticle(update, AuthorPrincipal);

		await using var context = GetContext();
		Assert.Multiple(() => {
			var article = context.Set<Article>().IgnoreQueryFilters().First(a => a.Id == TestArticleId);
			Assert.That(article.BodyHtml, Is.EqualTo(expectedHtml));
			Assert.That(article.BodyPlain, Is.EqualTo(expectedPlain));
		});
	}

	[Test]
	public async Task UpdateCategories_Success() {
		var update = GetValidTestArticle() with { Categories = [SecondaryCategoryId] };
		await Repository.UpdateArticle(update, AuthorPrincipal);

		await using var context = GetContext();
		Assert.Multiple(() => {
			var article = context.Set<Article>().IgnoreQueryFilters()
				.Include(a => a.Categories).First(a => a.Id == TestArticleId);
			Assert.That(article.Categories, Has.Count.EqualTo(1));
			Assert.That(article.Categories.First().Id, Is.EqualTo(SecondaryCategoryId));
		});
	}

	#endregion

	#region Permission Tests
	
	[Test]
	public void AnonymousUser_ThrowsMissingPermissions() {
		var update = GetValidTestArticle();

		Assert.ThrowsAsync<ArticleMissingPermissionsException>(
			async () => await Repository.UpdateArticle(update, AnonymousPrincipal));
	}

	[Test]
	public void RegularUser_ThrowsMissingPermissions() {
		var update = GetValidTestArticle();

		Assert.ThrowsAsync<ArticleMissingPermissionsException>(
			async () => await Repository.UpdateArticle(update, UserPrincipal));
	}
	
	[Test]
	public void UnrelatedAuthor_ThrowsMissingPermissions() {
		var update = GetValidTestArticle();

		Assert.ThrowsAsync<ArticleMissingPermissionsException>(
			async () => await Repository.UpdateArticle(update, ReviewerPrincipal));
	}

	#endregion

	#region Data Validation Tests

	[Test]
	public void SlugLength65_ThrowsMalformed() {
		var update = GetValidTestArticle() with { Slug = StringOfLength(65) };
		Assert.ThrowsAsync<ArticleMalformedException>(
			async () => await Repository.UpdateArticle(update, AuthorPrincipal));
	}
	
	[Test]
	public void TitleLength257_ThrowsMalformed() {
		var update = GetValidTestArticle() with { Slug = StringOfLength(257) };
		Assert.ThrowsAsync<ArticleMalformedException>(
			async () => await Repository.UpdateArticle(update, AuthorPrincipal));
	}

	#endregion
}