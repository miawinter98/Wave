using System.ComponentModel.DataAnnotations;

namespace Wave.Data.Transactional;

public abstract record ArticleDto(
	[property:MaxLength(64)]
	string? Slug,
	DateTimeOffset? PublishDate,
	Guid[]? Categories,
	Guid[]? Images);

public record ArticleCreateDto(
	[property:Required(AllowEmptyStrings = false)]
	[property:MaxLength(256)]
	string Title,
	[property:Required(AllowEmptyStrings = false)] 
	string Body,
	string? Slug,
	DateTimeOffset? PublishDate,
	Guid[]? Categories,
	Guid[]? Images) : ArticleDto(Slug, PublishDate, Categories, Images);
public record ArticleUpdateDto(
	[property:Required]
	Guid Id,
	[property:MaxLength(256)]
	string? Title = null,
	string? Body = null,
	string? Slug = null,
	DateTimeOffset? PublishDate = null,
	Guid[]? Categories = null,
	Guid[]? Images = null) : ArticleDto(Slug, PublishDate, Categories, Images);