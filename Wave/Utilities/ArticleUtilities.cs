using Wave.Data;

namespace Wave.Utilities;

public static class ArticleUtilities {
	public static string GenerateArticleLink(Article article, Uri? host) {
		string link;
		if (article.PublishDate.Year >= 9999) {
			link = $"/article/{article.Id}";
		} else {
			string titleEncoded = Uri.EscapeDataString(article.Title.ToLowerInvariant()).Replace("-", "+").Replace("%20", "-");
			link = $"/{article.PublishDate.Year}/{article.PublishDate.Month:D2}/{article.PublishDate.Day:D2}/{titleEncoded}";
		}
		return (host != null ? new Uri(host, link) : new Uri(link)).AbsoluteUri;
	}
}