using System.Security.Claims;
using Wave.Data;

namespace Wave.Utilities;

/// <summary>
/// Central location for assessing if a user has access to an article, or may modify them in specific ways
/// </summary>
public static class Permissions {
	public static bool AllowedToRead(this Article? article, ClaimsPrincipal principal) {
		if (article is null || article.IsDeleted) return false;
		if (article.Author is null) throw new ArgumentException("Checking permissions without loading related Author.");
		
		// The Article is publicly available
		if (article.Status >= ArticleStatus.Published && article.PublishDate <= DateTimeOffset.UtcNow) {
			return true;
		}
		
		// Admins always get access
		if (principal.IsInRole("Admin")) {
			return true;
		}
		
		// You can only access your own drafts
		if (article.Status is ArticleStatus.Draft && article.Author.Id == principal.FindFirst("Id")!.Value) {
			return true;
		}

		// Reviewers can see in-review articles
		if (article.Status is ArticleStatus.InReview && principal.IsInRole("Reviewer")) {
			return true;
		}

		return false;
	}

	public static bool AllowedToEdit(this Article? article, ClaimsPrincipal principal) {
		if (article is null || article.IsDeleted) return false;
		if (article.Author is null) throw new ArgumentException("Checking permissions without loading related Author.");

		// Admins always can edit articles
		if (principal.IsInRole("Admin")) {
			return true;
		}

		// You can edit your own draft articles
		if (article.Status is ArticleStatus.Draft && article.Author.Id == principal.FindFirst("Id")!.Value) {
			return true;
		}

		// Reviewers can edit in-review articles
		if (article.Status is ArticleStatus.InReview && principal.IsInRole("Reviewer")) {
			// Nobody is reviewing this article yet
			if (article.Reviewer is null || article.Reviewer.Id == article.Author.Id) {
				return true;
			} 
			// This reviewer is the Reviewer of the article 
			if (article.Reviewer?.Id == principal.FindFirst("Id")!.Value) {
				return true;
			}
			return false;
		}

		// Moderators can edit published/-ing articles
		if (article.Status is ArticleStatus.Published && principal.IsInRole("Moderator")) {
			return true;
		}

		return false;
	}

	public static bool AllowedToRejectReview(this Article? article, ClaimsPrincipal principal) {
		// if you can publish it, you can reject it
		return article?.Status is ArticleStatus.InReview && article.AllowedToPublish(principal);
	}

	public static bool AllowedToSubmitForReview(this Article? article, ClaimsPrincipal principal) {
		if (article is null || article.IsDeleted) return false;
		if (article.Author is null) throw new ArgumentException("Checking permissions without loading related Author.");

		// Draft articles can be submitted by their authors (admins can publish them anyway, no need to submit)
		if (article.Status is ArticleStatus.Draft && article.Author.Id == principal.FindFirst("Id")!.Value) {
			return true;
		}

		return false;
	}

	public static bool AllowedToPublish(this Article? article, ClaimsPrincipal principal) {
		if (article is null || article.IsDeleted) return false;
		if (article.Author is null) throw new ArgumentException("Checking permissions without loading related Author.");
		
		// Admins can skip review and directly publish draft articles
		if (article.Status is ArticleStatus.Draft && principal.IsInRole("Admin")) {
			return true;
		}

		// Admins may always review articles
		if (article.Status is ArticleStatus.InReview && principal.IsInRole("Admin")) {
			return true;
		}

		// Reviewers can review in-review articles, as long as they are not their own
		if (article.Status is ArticleStatus.InReview && principal.IsInRole("Reviewer") &&
		    article.Author.Id != principal.FindFirst("Id")!.Value) {
			return true;
		}

		return false;
	}

	public static bool AllowedToDelete(this Article? article, ClaimsPrincipal principal) {
		if (article is null || article.IsDeleted) return false;
		if (article.Author is null) throw new ArgumentException("Checking permissions without loading related Author.");

		// Admins can delete articles whenever
		if (principal.IsInRole("Admin")) {
			return true;
		}

		// You can delete your drafts
		if (article.Status is ArticleStatus.Draft && article.Author.Id == principal.FindFirst("Id")!.Value) {
			return true;
		}
		
		// Reviewers can reject/delete in-review articles
		if (article.Status is ArticleStatus.InReview && principal.IsInRole("Reviewer")) {
			// Nobody is reviewing this article yet
			if (article.Reviewer is null || article.Reviewer.Id == article.Author.Id) {
				return true;
			} 
			// This reviewer is the Reviewer of the article 
			if (article.Reviewer?.Id == principal.FindFirst("Id")!.Value) {
				return true;
			}
			return false;
		}

		// Moderators can take down articles
		if (article.Status is ArticleStatus.Published && principal.IsInRole("Moderator")) {
			return true;
		}

		return false;
	}
}