namespace Wave.Data.Transactional;

public sealed record ArticleView(
	Guid Id, 
	string Title, 
	string Slug,
	string BodyHtml, 
	string Body, 
	string BodyPlain,
	ArticleStatus Status,
	DateTimeOffset PublishDate) 
{
	public ArticleView(Article article) : this(
		article.Id, 
		article.Title, 
		article.Slug,
		article.BodyHtml,
		article.Body,
		article.BodyPlain,
		article.Status,
		article.PublishDate) {}
}