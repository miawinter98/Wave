using System.Collections.Frozen;
using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using MimeKit;
using StackExchange.Redis;
using Wave.Data;
using Wave.Utilities;

namespace Wave.Services;

public class EmailFactory(IOptions<Customization> customizations, EmailTemplateService templateService) {
	private Customization Customizations { get; } = customizations.Value;
	private EmailTemplateService TemplateService { get; } = templateService;

	public async ValueTask<IEmail> CreateDefaultEmail(string receiverMail, string? receiverName, string subject, string title, string bodyHtml, string bodyPlain = "") {
		(string host, string logo) = GetStaticData();
		string body = await TemplateService.DefaultAsync(host, logo, title, bodyHtml);

		return new StaticEmail(receiverMail, receiverName, subject, title, body, $"{title}\n\n{bodyPlain}", FrozenDictionary<string, string>.Empty);
	}

	public async ValueTask<IEmail> CreateConfirmationEmail(EmailSubscriber subscriber, string subject, string title, string bodyHtml, string bodyPlain = "", string confirmLabel = "confirm") {
		(string user, string token) = await TemplateService.CreateConfirmTokensAsync(subscriber.Id);
		(string? host, string _) = GetStaticData();
		string confirmLink = new Uri(
			new Uri(host, UriKind.Absolute), 
			new Uri($"/Email/Confirm?user={WebUtility.UrlEncode(user)}&token={WebUtility.UrlEncode(token)}", UriKind.Relative))
			.AbsoluteUri;

		return await CreateDefaultEmail(subscriber.Email, subscriber.Name, subject, title, 
			string.Format(bodyHtml, Customizations.AppName) 
			+ $"""<p style="text-align: center"><a href="{confirmLink}">{confirmLabel}</a></p>""", 
			string.Format(bodyPlain, Customizations.AppName)  + $"\n\n{confirmLabel}: {confirmLink}");
	}

	public async ValueTask<IEmail> CreateSubscribedEmail(EmailSubscriber subscriber, string browserLink, string subject, string title, string bodyHtml, string bodyPlain = "", string role = "unknown", string? replyTo = null) {
		(string host, string logo) = GetStaticData();

		string unsubscribeLink = await GetUnsubscribeLink(host, subscriber.Id, role);
		string body = await TemplateService.NewsletterAsync(host, browserLink, logo, title, bodyHtml, unsubscribeLink);
		string footer = await TemplateService.GetPartialAsync("email-plain-footer");
		bodyPlain += "\n\n" + footer.Replace(
			$"[[{EmailTemplateService.Constants.EmailUnsubscribeLink}]]", 
			unsubscribeLink, true, CultureInfo.InvariantCulture);

		var headers = new Dictionary<string, string>{
			{HeaderId.ListUnsubscribe.ToHeaderName(), $"<{unsubscribeLink}>"},
			{HeaderId.ListUnsubscribePost.ToHeaderName(), "One-Click"}
		};
		if (!string.IsNullOrWhiteSpace(replyTo)) headers.Add(HeaderId.ReplyTo.ToHeaderName(), replyTo);
		return new StaticEmail(subscriber.Email, subscriber.Name, subject, title, body, $"{title}\n\n{bodyPlain}", headers.ToFrozenDictionary());
	}

	public async ValueTask<IEmail> CreateWelcomeEmail(EmailSubscriber subscriber, IEnumerable<EmailNewsletter> articles, string subject, string title, string bodyHtml, string bodyPlain = "") {
		(string host, string logo) = GetStaticData();

		string articlePartial = await TemplateService.GetPartialAsync("email-article");
		string footer = await TemplateService.GetPartialAsync("email-plain-footer");
		var articlesHtml = new StringBuilder("");
		var articlesPlain = new StringBuilder("");
		foreach (var n in articles) {
			string articleLink = ArticleUtilities.GenerateArticleLink(n.Article, new Uri(Customizations.AppUrl, UriKind.Absolute));
			articlesHtml.AppendFormat(
				articlePartial, 
				n.Article.Title, n.Article.Author.Name, n.Article.Body[..Math.Min(250, n.Article.Body.Length)], articleLink);
			articlesPlain.AppendFormat("{0}\n\n{1}\n{2}\n{3}",
				n.Article.Title, n.Article.Author.Name, n.Article.Body[..Math.Min(250, n.Article.Body.Length)], articleLink);
		}
		
		string unsubscribeLink = await GetUnsubscribeLink(host, subscriber.Id, "welcome");
		string body = TemplateService.Welcome(host, logo, title, bodyHtml, unsubscribeLink, articlesHtml.ToString());
		bodyPlain += "\n\n\n" + articlesPlain;
		bodyPlain += "\n\n" + footer.Replace(
			$"[[{EmailTemplateService.Constants.EmailUnsubscribeLink}]]", 
			unsubscribeLink, true, CultureInfo.InvariantCulture);
		
		return new StaticEmail(subscriber.Email, subscriber.Name, subject, title, body, $"{title}\n\n{bodyPlain}", new Dictionary<string, string>{
			{HeaderId.ListUnsubscribe.ToHeaderName(), $"<{unsubscribeLink}>"},
			{HeaderId.ListUnsubscribePost.ToHeaderName(), "One-Click"}
		}.ToFrozenDictionary());
	}

	public async ValueTask<Guid?> IsTokenValid(string id, string token) {
		return await TemplateService.ValidateTokensAsync(id, token, deleteToken: false);
	}

	public async ValueTask ClearToken(string id, string token) {
		await TemplateService.ValidateTokensAsync(id, token, deleteToken: true);
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