using System.ComponentModel.DataAnnotations;

namespace Wave.Data;

public enum ArticleStatus {
    Draft = 0, 
    InReview = 1, 
    Published = 2
}

// TODO:: Add tags for MVP ?
// TODO:: Archive System (Notice / Redirect to new content?) (Deprecation date?)

public class Article : ISoftDelete {
    [Key]
    public Guid Id { get; set; }
    public bool IsDeleted { get; set; }

    [MaxLength(256)]
    public required string Title { get; set; }
    public required string Body { get; set; }
    public string BodyHtml { get; set; } = string.Empty;
    public string BodyPlain { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Slug { get; set; } = string.Empty;

    public required ApplicationUser Author { get; set; }
    public ApplicationUser? Reviewer { get; set; }

    public ArticleStatus Status { get; set; }
    public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset PublishDate { get; set; } = DateTimeOffset.MaxValue;
    public DateTimeOffset? LastModified { get; set; }

    public IList<Category> Categories { get; } = [];
    public IList<ArticleImage> Images { get; } = [];
}