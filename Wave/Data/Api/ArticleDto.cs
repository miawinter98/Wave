using Wave.Utilities;

namespace Wave.Data.Api;

public record ArticleDto(
	string Title,
	string ContentPreview,
	string BrowserUrl,
	DateTimeOffset PublishDate,
	UserDto Author,
	UserDto? Reviewer,
	IList<CategoryDto> Categories) {

	public static ArticleDto GetFromArticle(Article article, Uri host, int pfpSize) {
		string browserLink = ArticleUtilities.GenerateArticleLink(article, host);

		var author = UserDto.GetFromUser(article.Author, host, pfpSize);
		var reviewer = 
			article.Reviewer is not null && 
			article.Reviewer.Id != article.Author.Id 
				? UserDto.GetFromUser(article.Reviewer, host, pfpSize) : null;

		var categories = article.Categories.Select(c => new CategoryDto(c)).ToArray();
		string preview = article.BodyPlain[..Math.Min(article.BodyPlain.Length, 500)];

		return new ArticleDto(article.Title, preview, browserLink, article.PublishDate, author, reviewer, categories);
	}
}