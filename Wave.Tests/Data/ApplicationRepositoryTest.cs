﻿using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Wave.Data;
using Wave.Data.Transactional;
using Wave.Tests.TestUtilities;

// ReSharper disable InconsistentNaming

namespace Wave.Tests.Data;

public abstract class ApplicationRepositoryTests : DbContextTest {
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
			new Category {
				Name = "Secondary Category",
				Color = CategoryColors.Secondary
			}
		];

		await using var context = GetContext();
		var user = new ApplicationUser {
			UserName = TestUserName
		};
		var author = new ApplicationUser {
			UserName = AuthorUserName
		};
		var reviewer = new ApplicationUser {
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

	private sealed class MockDbContextFactory(Func<ApplicationDbContext> supplier)
		: IDbContextFactory<ApplicationDbContext> {
		public ApplicationDbContext CreateDbContext() => supplier();
	}
}

[TestFixture, FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[TestOf(typeof(ApplicationRepository))]
public class ApplicationRepositoryTest_GetCategories : ApplicationRepositoryTests {

	#region Success Tests

	[Test]
	public async Task AnonymousDefaultOneArticleOneCategory_Success() {
		await Repository.CreateArticleAsync(
			new ArticleCreateDto("test", "*test*", null, null, [PrimaryCategoryId], null), AuthorPrincipal);

		var result = await Repository.GetCategories(AnonymousPrincipal);
		Assert.Multiple(() => {
			Assert.That(result, Is.Not.Empty);
			Assert.That(result.First().Id, Is.EqualTo(PrimaryCategoryId));
			Assert.That(result, Has.Count.EqualTo(1));
		});
	}

	#endregion

	#region Permission Tests

	[Test]
	public async Task AnonymousDefaultNoArticles_Success() {
		var result = await Repository.GetCategories(AnonymousPrincipal);

		Assert.Multiple(() => { Assert.That(result, Is.Empty); });
	}

	[Test]
	public void AnonymousNoArticlesAllCategories_ThrowsMissingPermissions() {
		Assert.ThrowsAsync<ApplicationException>(async () => await Repository.GetCategories(AnonymousPrincipal, true));
	}

	[Test]
	public async Task AuthorDefaultNoArticles_Success() {
		var result = await Repository.GetCategories(AuthorPrincipal);

		Assert.Multiple(() => { Assert.That(result, Is.Empty); });
	}

	[Test]
	public async Task AuthorDefaultNoArticlesAllCategories_Success() {
		var result = await Repository.GetCategories(AuthorPrincipal, true);

		Assert.Multiple(() => {
			Assert.That(result, Is.Not.Empty);
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result.FirstOrDefault(c => c.Id == PrimaryCategoryId), Is.Not.Null);
			Assert.That(result.FirstOrDefault(c => c.Id == SecondaryCategoryId), Is.Not.Null);
		});
	}

	#endregion

}

[TestFixture, FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[TestOf(typeof(ApplicationRepository))]
public class ApplicationRepositoryTest_CreateArticle : ApplicationRepositoryTests {
	private static ArticleCreateDto GetValidTestArticle(Guid[]? categories = null) {
		return new ArticleCreateDto(
			"Test Article",
			"*Test* Body",
			null, null, categories, null);
	}

	#region Success Tests

	[Test]
	public async Task MinimalArticle_Success() {
		var article = GetValidTestArticle();

		var view = await Repository.CreateArticleAsync(article, AuthorPrincipal);

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
		var article = GetValidTestArticle([PrimaryCategoryId]);
		var view = await Repository.CreateArticleAsync(article, AuthorPrincipal);

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
			async () => await Repository.CreateArticleAsync(article, UserPrincipal));
	}

	[Test]
	public void AnonymousUser_ThrowsMissingPermissions() {
		var article = GetValidTestArticle();

		Assert.ThrowsAsync<ArticleMissingPermissionsException>(
			async () => await Repository.CreateArticleAsync(article, AnonymousPrincipal));
	}

	#endregion

	#region Data Validation Tests

	[Test]
	public void MissingTitle_ThrowsMalformed() {
		var article = new ArticleCreateDto(null!, "test", null, null, null, null);

		Assert.ThrowsAsync<ArticleMalformedException>(
			async () => await Repository.CreateArticleAsync(article, AuthorPrincipal));
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

		var view = await Repository.CreateArticleAsync(testArticle, AuthorPrincipal);
		TestArticleId = view.Id;
	}

	#region Success Tests

	[Test]
	public async Task UpdateTitle_Success() {
		var update = new ArticleUpdateDto(TestArticleId, "New Title");

		await Repository.UpdateArticleAsync(update, AuthorPrincipal);

		await using var context = GetContext();
		Assert.Multiple(() => {
			Assert.That(context.Set<Article>().IgnoreQueryFilters().First(a => a.Id == TestArticleId).Title,
				Is.EqualTo("New Title"));
		});
	}

	[Test]
	public async Task UpdateBodyUpdatesHtmlAndPlain_Success() {
		var update = new ArticleUpdateDto(TestArticleId, body:"Some *new* Body");
		const string expectedHtml = "<p>Some <em>new</em> Body</p>";
		const string expectedPlain = "Some new Body";

		await Repository.UpdateArticleAsync(update, AuthorPrincipal);

		await using var context = GetContext();
		Assert.Multiple(() => {
			var article = context.Set<Article>().IgnoreQueryFilters().First(a => a.Id == TestArticleId);
			Assert.That(article.BodyHtml, Is.EqualTo(expectedHtml));
			Assert.That(article.BodyPlain, Is.EqualTo(expectedPlain));
		});
	}

	[Test]
	public async Task UpdateCategories_Success() {
		var update = new ArticleUpdateDto(TestArticleId, categories:[SecondaryCategoryId]);
		await Repository.UpdateArticleAsync(update, AuthorPrincipal);

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
			async () => await Repository.UpdateArticleAsync(update, AnonymousPrincipal));
	}

	[Test]
	public void RegularUser_ThrowsMissingPermissions() {
		var update = GetValidTestArticle();

		Assert.ThrowsAsync<ArticleMissingPermissionsException>(
			async () => await Repository.UpdateArticleAsync(update, UserPrincipal));
	}

	[Test]
	public void UnrelatedAuthor_ThrowsMissingPermissions() {
		var update = GetValidTestArticle();

		Assert.ThrowsAsync<ArticleMissingPermissionsException>(
			async () => await Repository.UpdateArticleAsync(update, ReviewerPrincipal));
	}

	#endregion

	#region Data Validation Tests

	[Test]
	public void SlugLength65_ThrowsMalformed() {
		var update = new ArticleUpdateDto(TestArticleId, slug:StringOfLength(65));
		Assert.ThrowsAsync<ArticleMalformedException>(
			async () => await Repository.UpdateArticleAsync(update, AuthorPrincipal));
	}

	[Test]
	public void TitleLength257_ThrowsMalformed() {
		var update = new ArticleUpdateDto(TestArticleId, slug:StringOfLength(257));
		Assert.ThrowsAsync<ArticleMalformedException>(
			async () => await Repository.UpdateArticleAsync(update, AuthorPrincipal));
	}

	#endregion

}