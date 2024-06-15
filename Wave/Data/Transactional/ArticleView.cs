namespace Wave.Data.Transactional;

public sealed record CategoryView(Guid id, string Name, CategoryColors Color) {
	public CategoryView(Category category) : this(
		category.Id, category.Name, category.Color) {}

}

public sealed record ArticleView(
	Guid Id, 
	string Title, 
	string Slug,
	string BodyHtml, 
	string Body, 
	string BodyPlain,
	ArticleStatus Status,
	DateTimeOffset PublishDate,
	IReadOnlyList<CategoryView> Categories) 
{
	public ArticleView(Article article) : this(
		article.Id, 
		article.Title, 
		article.Slug,
		article.BodyHtml,
		article.Body,
		article.BodyPlain,
		article.Status,
		article.PublishDate,
		article.Categories.Select(c => new CategoryView(c)).ToList()) {}
}