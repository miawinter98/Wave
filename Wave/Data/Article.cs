using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
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

	public void UpdateSlug(string? potentialNewSlug = null) {
		if (!string.IsNullOrWhiteSpace(potentialNewSlug) && Uri.IsWellFormedUriString(potentialNewSlug, UriKind.Relative)) {
			Slug = potentialNewSlug;
			return;
		}

		if (string.IsNullOrWhiteSpace(potentialNewSlug) && !string.IsNullOrWhiteSpace(Slug)) return;

		string baseSlug = potentialNewSlug ?? Title;
		baseSlug = baseSlug.ToLowerInvariant()[..Math.Min(64, baseSlug.Length)];
		string slug = Uri.EscapeDataString(baseSlug).Replace("-", "+").Replace("%20", "-");
		
		// I hate my life
		int escapeTrimOvershoot = 0;
		if (slug.Length > 64) {
			// Escape sequences come with a % and two hex digits, there may be up to 3 of such sequences
			// per character escaping ('?' has %3F, but € has %E2%82%AC), so we need to find the last group
			// of such an escape parade and see if it's going over by less than 9, because then we need to 
			// remove more characters in the truncation, or we end up with a partial escape sequence.. parade
			escapeTrimOvershoot = 64 - Regex.Match(slug,
					@"(?<escape>(%[a-fA-F\d][a-fA-F\d])+)",
					RegexOptions.None | RegexOptions.ExplicitCapture)
				.Groups.Values.Last(g => g.Index < 64).Index;
			if (escapeTrimOvershoot > 9) escapeTrimOvershoot = 0;
		}

		Slug = slug[..Math.Min(slug.Length, 64 - escapeTrimOvershoot)];
		if (Slug.EndsWith("%")) Slug = Slug[..^1];
	}

	public void UpdateBody() {
		BodyHtml = MarkdownUtilities.Parse(Body).Trim();
		BodyPlain = HtmlUtilities.GetPlainText(BodyHtml).Trim();
	}
}