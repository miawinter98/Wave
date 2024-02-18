using System.Collections.Frozen;
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using MimeKit;
using Wave.Data;
using Wave.Utilities;

namespace Wave.Services;

public class EmailFactory(IOptions<Customization> customizations, EmailTemplateService templateService) {
	private Customization Customizations { get; } = customizations.Value;
	private EmailTemplateService TemplateService { get; } = templateService;

	public async ValueTask<IEmail> CreateDefaultEmail(string receiverMail, string? receiverName, string subject, string title, string bodyHtml) {
		(string host, string logo) = GetStaticData();
		string body = await TemplateService.DefaultAsync(host, logo, title, bodyHtml);

		return new StaticEmail(receiverMail, receiverName, subject, title, body, FrozenDictionary<string, string>.Empty);
	}

	public async ValueTask<IEmail> CreateSubscribedEmail(EmailSubscriber subscriber, string browserLink, string subject, string title, string bodyHtml, string role = "unknown", string? replyTo = null) {
		(string host, string logo) = GetStaticData();

		string unsubscribeLink = await GetUnsubscribeLink(host, subscriber.Id, role);
		string body = await TemplateService.NewsletterAsync(host, browserLink, logo, title, bodyHtml, unsubscribeLink);

		var headers = new Dictionary<string, string>{
			{HeaderId.ListUnsubscribe.ToHeaderName(), $"<{unsubscribeLink}>"},
			{HeaderId.ListUnsubscribePost.ToHeaderName(), "One-Click"}
		};
		if (!string.IsNullOrWhiteSpace(replyTo)) headers.Add(HeaderId.ReplyTo.ToHeaderName(), replyTo);
		return new StaticEmail(subscriber.Email, subscriber.Name, subject, title, body, headers.ToFrozenDictionary());
	}

	public async ValueTask<IEmail> CreateWelcomeEmail(EmailSubscriber subscriber, IEnumerable<EmailNewsletter> articles, string subject, string title, string bodyHtml) {
		(string host, string logo) = GetStaticData();

		string articlePartial = await TemplateService.GetPartialAsync("email-article");
		var articlesHtml = new StringBuilder("");
		foreach (var n in articles) {
			string articleLink = ArticleUtilities.GenerateArticleLink(n.Article, new Uri(Customizations.AppUrl, UriKind.Absolute));
			articlesHtml.AppendFormat(
				articlePartial, 
				n.Article.Title, n.Article.Author.Name, n.Article.Body[..Math.Min(250, n.Article.Body.Length)], articleLink);
		}
		
		string unsubscribeLink = await GetUnsubscribeLink(host, subscriber.Id, "welcome");
		string body = TemplateService.Welcome(host, logo, title, bodyHtml, unsubscribeLink, articlesHtml.ToString());
		
		return new StaticEmail(subscriber.Email, subscriber.Name, subject, title, body, new Dictionary<string, string>{
			{HeaderId.ListUnsubscribe.ToHeaderName(), $"<{unsubscribeLink}>"},
			{HeaderId.ListUnsubscribePost.ToHeaderName(), "One-Click"}
		}.ToFrozenDictionary());
	}

	private (string host, string logo) GetStaticData() {
		var host = new Uri(string.IsNullOrWhiteSpace(Customizations.AppUrl) ? "" : Customizations.AppUrl); // TODO get link
		string logo = !string.IsNullOrWhiteSpace(Customizations.LogoLink)
			? Customizations.LogoLink
			: new Uri(host, "/img/logo.png").AbsoluteUri;
		return (host.AbsoluteUri, logo);
	}

	private async ValueTask<string> GetUnsubscribeLink(string host, Guid subscriberId, string role) {
		(string user, string token) = await TemplateService.CreateConfirmTokensAsync(subscriberId, "unsubscribe-"+role, TimeSpan.FromDays(30));
		return new Uri(new Uri(host), $"/Email/Unsubscribe?newsletter={role}&user={WebUtility.UrlEncode(user)}&token={WebUtility.UrlEncode(token)}").AbsoluteUri;
	}
}