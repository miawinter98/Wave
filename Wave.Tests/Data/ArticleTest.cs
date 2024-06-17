using Wave.Data;

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
		Assert.That(Article.Slug, Is.EqualTo("title-with-special-characters"));
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
		Assert.That(Article.Slug, Is.EqualTo("auto-generating-slugs-was-a-mistake-i-hate-this-______-"));
	}
	
	[Test]
	public void SlugFromTitleLongerThan64CharacterWithSpecialCharacterEscapeSize2AtPosition56() {
		Article.Title = "Auto generating slugs was a mistake I hate this _______ üa";
		Article.UpdateSlug();
		Assert.That(Article.Slug, Is.EqualTo("auto-generating-slugs-was-a-mistake-i-hate-this-_______-a"));
	}

	[Test]
	public void SlugFromTitleLongerThan64CharacterWithSpecialCharacterEscapeSize3AtPosition56() {
		Article.Title = "Auto generating slugs was a mistake I hate this _______ €";
		Article.UpdateSlug();
		Assert.That(Article.Slug, Is.EqualTo("auto-generating-slugs-was-a-mistake-i-hate-this-_______-a"));
	}
	
	[Test]
	public void SlugFromTitleLongerThan64CharacterWithSpecialCharacterEscapeSize2AtPosition57() {
		Article.Title = "Auto generating slugs was a mistake I hate this ________ üa";
		Article.UpdateSlug();
		Assert.That(Article.Slug, Is.EqualTo("auto-generating-slugs-was-a-mistake-i-hate-this-________-a"));
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
		Assert.That(Article.Slug, Is.EqualTo("article-that-ends-with-a-special-character-and-need-special-cre"));
	}
	
	[Test]
	public void SlugFromTitleLongerThan64CharacterWithSpecialCharacterAtPosition62() {
		Article.Title = "Article that ends with a special character and needs special cäre";
		Article.UpdateSlug();
		Assert.That(Article.Slug, Is.EqualTo("article-that-ends-with-a-special-character-and-needs-special-cre"));
	}
	
	[Test]
	public void SlugFromTitleLongerThan64CharacterWithSpecialCharacterAtPosition63() {
		Article.Title = "Article that ends with a special character and needs special caäre";
		Article.UpdateSlug();
		Assert.That(Article.Slug, Is.EqualTo("article-that-ends-with-a-special-character-and-needs-special-car"));
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