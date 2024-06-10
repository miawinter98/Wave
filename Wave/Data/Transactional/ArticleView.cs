namespace Wave.Data.Transactional;

public sealed record ArticleView(
	Guid Id, 
	string Title, 
	string Slug,
	string Html, 
	string Text,
	ArticleStatus Status,
	DateTimeOffset PublishDate) 
{
	public ArticleView(Article article) : this(
		article.Id, 
		article.Title, 
		article.Slug,
		article.BodyHtml,
		article.BodyPlain,
		article.Status,
		article.PublishDate) {}
}