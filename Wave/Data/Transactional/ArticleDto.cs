using System.ComponentModel.DataAnnotations;

namespace Wave.Data.Transactional;

public abstract class ArticleDto(
	string? slug,
	DateTimeOffset? publishDate,
	Guid[]? categories,
	Guid[]? images) 
{
	[MaxLength(64)]
	public string? Slug { get; init; } = slug;

	public DateTimeOffset? PublishDate { get; init; } = publishDate;
	public Guid[]? Categories { get; init; } = categories;
	public Guid[]? Images { get; init; } = images;

	public void Deconstruct(
		out string? slug, out DateTimeOffset? publishDate, out Guid[]? categories, out Guid[]? images) {
		slug = Slug;
		publishDate = PublishDate;
		categories = Categories;
		images = Images;
	}
}

public class ArticleCreateDto(
	string title,
	[Required(AllowEmptyStrings = false)] string body,
	string? slug,
	DateTimeOffset? publishDate,
	Guid[]? categories,
	Guid[]? images) : ArticleDto(slug, publishDate, categories, images) 
{
	[Required(AllowEmptyStrings = false)]
	[MaxLength(256)]
	public string Title { get; init; } = title;

	public string Body { get; init; } = body;

	public void Deconstruct(
		out string title, out string body, out string? slug, out DateTimeOffset? publishDate, out Guid[]? categories,
		out Guid[]? images) {
		title = Title;
		body = Body;
		slug = Slug;
		publishDate = PublishDate;
		categories = Categories;
		images = Images;
	}
}

public class ArticleUpdateDto(
	Guid id,
	string? title = null,
	string? body = null,
	string? slug = null,
	DateTimeOffset? publishDate = null,
	Guid[]? categories = null,
	Guid[]? images = null) : ArticleDto(slug, publishDate, categories, images) 
{
	[Required]
	public Guid Id { get; init; } = id;

	[MaxLength(256)]
	public string? Title { get; init; } = title;

	public string? Body { get; init; } = body;

	public void Deconstruct(
		out Guid id, out string? title, out string? body, out string? slug, out DateTimeOffset? publishDate,
		out Guid[]? categories, out Guid[]? images) {
		id = Id;
		title = Title;
		body = Body;
		slug = Slug;
		publishDate = PublishDate;
		categories = Categories;
		images = Images;
	}
}