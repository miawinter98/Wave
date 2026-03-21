using wave.server.Data;
using wave.server.Endpoints.User;

namespace wave.server.Endpoints.Article;

public record ArticleResponse(
    string Identifier,
    string Title,
    string Body,
    string Url,
    UserDto Author,
    ArticleStatus Status,
    DateTimeOffset Created,
    DateTimeOffset? Modified,
    DateTimeOffset Published
)
{
    public ArticleResponse(ArticleRecord record)
        : this(
            $"/api/article/{record.Id}",
            record.Title,
            record.Body,
            $"/api/article/{record.Slug}",
            new UserDto(record.Author),
            record.Status,
            record.CreationDate,
            record.LastModified,
            record.PublishDate
        ) { }
}
