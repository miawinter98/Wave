using System.ComponentModel.DataAnnotations;
using Wave.Utilities;

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

	// Computed 
	public bool CanBePublic { get; set; }

	[MaxLength(256)]
	public required string Title { get; set; }
	// ReSharper disable thrice EntityFramework.ModelValidation.UnlimitedStringLength
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

	/// <summary>
	/// Returns LastModified if it's after the articles PublishDate, otherwise gives you the PublishDate
	/// </summary>
	public DateTimeOffset LastPublicChange => LastModified > PublishDate ? LastModified.Value : PublishDate;

	public IList<Category> Categories { get; } = [];
	public IList<ArticleImage> Images { get; } = [];

	public void UpdateSlug(string? potentialNewSlug) {
		if (string.IsNullOrWhiteSpace(potentialNewSlug) && !string.IsNullOrWhiteSpace(Slug)) return;
		
		string baseSlug = potentialNewSlug ?? Title;
		baseSlug = baseSlug.ToLowerInvariant()[..Math.Min(64, baseSlug.Length)];
		string slug = Uri.EscapeDataString(baseSlug).Replace("-", "+").Replace("%20", "-");
		// if our escaping increases the slug length, there is a chance it ends with an escape 
		// character, so if this overshoot is not divisible by 3, then we risk cutting of the 
		// escape character, so we need to remove it in it's entirely if that's the case
		int escapeTrimOvershoot = Math.Max(0, 3 - (slug.Length - baseSlug.Length) % 3);
		// if the slug already fits 64 character, there will be no cutoff in the next operation anyway,
		// so we don't need to fix what is described in the previous comment
		if (slug.Length <= 64) escapeTrimOvershoot = 0;
		Slug = slug[..Math.Min(slug.Length, 64 - escapeTrimOvershoot)];
	}

	public void UpdateBody() {
		BodyHtml = MarkdownUtilities.Parse(Body).Trim();
		BodyPlain = HtmlUtilities.GetPlainText(BodyHtml).Trim();
	}
}