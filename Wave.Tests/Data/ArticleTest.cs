using Wave.Data;
using Wave.Data.Migrations.postgres;

namespace Wave.Tests.Data;

[TestFixture, FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[TestOf(typeof(Article))]
public class ArticleTest {
	private Article Article { get; } = new() {
		Author = null!,
		Title = null!,
		Body = null!
	};

	[Test]
	public void SlugWithAscii() {
		Article.Title = "Testing Article";
		Article.UpdateSlug();
		Assert.That(Article.Slug, Is.EqualTo("testing-article"));
	}
	
	[Test]
	public void SlugWithSpecialCharacters() {
		Article.Title = "Title with, special characters?";
		Article.UpdateSlug();
		Assert.That(Article.Slug, Is.EqualTo("title-with%2C-special-characters%3F"));
	}

	[Test]
	public void SlugFromTitleLongerThan64Characters() {
		Article.Title = "Article Title that is longer than the sixty four character limit and should be truncated";
		Article.UpdateSlug();
		Assert.That(Article.Slug, Is.EqualTo("article-title-that-is-longer-than-the-sixty-four-character-limit"));
	}
	
	[Test]
	public void SlugFromTitleLongerThan64CharacterWithSpecialCharacterEscapeSize3AtPosition55() {
		Article.Title = "Auto generating slugs was a mistake I hate this ______ €";
		Article.UpdateSlug();
		Assert.That(Article.Slug, Is.EqualTo("auto-generating-slugs-was-a-mistake-i-hate-this-______-%E2%82%AC"));
	}

	[Test]
	public void SlugFromTitleLongerThan64CharacterWithSpecialCharacterEscapeSize3AtPosition56() {
		Article.Title = "Auto generating slugs was a mistake I hate this _______ €";
		Article.UpdateSlug();
		Assert.That(Article.Slug, Is.EqualTo("auto-generating-slugs-was-a-mistake-i-hate-this-_______-"));
	}

	[Test]
	public void SlugFromTitleLongerThan64CharacterWithSpecialCharacterEscapeSize3AtPosition57() {
		Article.Title = "Auto generating slugs was a mistake I hate this ________ €";
		Article.UpdateSlug();
		Assert.That(Article.Slug, Is.EqualTo("auto-generating-slugs-was-a-mistake-i-hate-this-________-"));
	}

	[Test]
	public void SlugFromTitleLongerThan64CharacterWithSpecialCharacterAtPosition61() {
		Article.Title = "Article that ends with a special character and need special cäre";
		Article.UpdateSlug();
		Assert.That(Article.Slug, Is.EqualTo("article-that-ends-with-a-special-character-and-need-special-c"));
	}
	
	[Test]
	public void SlugFromTitleLongerThan64CharacterWithSpecialCharacterAtPosition62() {
		Article.Title = "Article that ends with a special character and needs special cäre";
		Article.UpdateSlug();
		Assert.That(Article.Slug, Is.EqualTo("article-that-ends-with-a-special-character-and-needs-special-c"));
	}
	
	[Test]
	public void SlugFromTitleLongerThan64CharacterWithSpecialCharacterAtPosition63() {
		Article.Title = "Article that ends with a special character and needs special caäre";
		Article.UpdateSlug();
		Assert.That(Article.Slug, Is.EqualTo("article-that-ends-with-a-special-character-and-needs-special-ca"));
	}
	
	[Test]
	public void SlugProvidedValidUri() {
		Article.Title = "Testing providing a slug";
		Article.UpdateSlug("test-slug");
		Assert.That(Article.Slug, Is.EqualTo("test-slug"));
	}
	
	[Test]
	public void SlugProvidedNeedsEscaping() {
		Article.Title = "Testing providing a slug";
		Article.UpdateSlug("test slug");
		Assert.That(Article.Slug, Is.EqualTo("test-slug"));
	}
}