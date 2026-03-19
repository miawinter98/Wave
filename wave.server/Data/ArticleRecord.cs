namespace wave.server.Data;

public record ArticleRecord(Guid Id, bool IsDeleted, string Title, string Body, string Slug);
