using wave.server.Data;

namespace wave.server.Endpoints.Article;

public record ArticleResponse(string Title, string Body, string Url)
{
    public ArticleResponse(ArticleRecord record)
        : this(record.Title, record.Body, $"/api/article/{record.Slug}") { }
}
