using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace wave.server.Data;

[JsonConverter(typeof(JsonStringEnumConverter<ArticleStatus>))]
public enum ArticleStatus
{
    [UsedImplicitly(ImplicitUseKindFlags.Default)]
    Draft = 0,

    [UsedImplicitly(ImplicitUseKindFlags.Access)]
    InReview = 1,

    [UsedImplicitly(ImplicitUseKindFlags.Access)]
    Published = 2,
}

public record ArticleRecord(
    Guid Id,
    bool IsDeleted,
    string Title,
    string Body,
    string Slug,
    ArticleStatus Status
)
{
    public DateTimeOffset CreationDate { get; init; } = DateTimeOffset.Now;
    public DateTimeOffset PublishDate { get; init; } = DateTimeOffset.MaxValue;
    public DateTimeOffset? LastModified { get; init; }
}
