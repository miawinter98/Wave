using wave.server.Data;

namespace wave.server.Endpoints.Article;

public record ArticleResponse(
    string Title,
    string Body,
    string Url,
    ArticleStatus Status,
    DateTimeOffset Created,
    DateTimeOffset? Modified,
    DateTimeOffset Published
)
{
    public ArticleResponse(ArticleRecord record)
        : this(
            record.Title,
            record.Body,
            $"/api/article/{record.Slug}",
            record.Status,
            record.CreationDate,
            record.LastModified,
            record.PublishDate
        ) { }
}
