using NUnit.Framework.Constraints;
using Wave.Data;
using Wave.Utilities;

namespace Wave.Tests.Utilities;

[TestFixture]
[TestOf(typeof(ArticleUtilities))]
public class ArticleUtilitiesTest {

	[Test]
	public void GenerateArticleLink() {
		var testArticle = new Article {
			Id = Guid.Parse("e7a94905-d83a-4146-8061-de2ef7869a82"),
			Title = "Test Article",
			Body = "This is the body of the test Article",
			Author = new ApplicationUser {
				UserName = "test@example.com", 
				FullName = "Test User"
			},
			PublishDate = DateTimeOffset.MaxValue,
			Slug = "test-article"
		};

		string linkWithoutPublishDate = ArticleUtilities.GenerateArticleLink(testArticle, null);
		Assert.That(linkWithoutPublishDate, Is.EqualTo("/article/e7a94905-d83a-4146-8061-de2ef7869a82"));

		testArticle.PublishDate = new DateTimeOffset(new DateOnly(2024, 4, 24), TimeOnly.MinValue, TimeSpan.Zero);
		string linkWithPublishDate = ArticleUtilities.GenerateArticleLink(testArticle, null);
		Assert.That(linkWithPublishDate, Is.EqualTo("/2024/04/24/test-article"));
		
		string testHttps = ArticleUtilities.GenerateArticleLink(testArticle, new Uri("http://example.com", UriKind.Absolute));
		Assert.That(testHttps, new StartsWithConstraint("https://"));
	}
}