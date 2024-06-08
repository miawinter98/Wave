using System.ComponentModel.DataAnnotations;

namespace Wave.Data.Transactional;

public record ArticleDto(
	Guid? Id,
	[property:Required(AllowEmptyStrings = false)]
	[property:MaxLength(256)]
	string Title,
	[property:Required(AllowEmptyStrings = false)] 
	string Body,
	[property:MaxLength(64)]
	string? Slug,
	DateTimeOffset? PublishDate,
	Guid[]? Categories,
	Guid[]? Images);