using System.Net;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;
using Wave.Data;
using Wave.Utilities;
using Uri = System.Uri;

namespace Wave.Services;

public class SmtpEmailSender(ILogger<SmtpEmailSender> logger, IOptions<SmtpConfiguration> config, IOptions<Customization> customizations, EmailTemplateService templateService, FileSystemService fileSystemService) : IEmailSender<ApplicationUser>, IAdvancedEmailSender {
	private ILogger<SmtpEmailSender> Logger { get; } = logger;
	private SmtpConfiguration Configuration { get; } = config.Value;
	private Customization Customizations { get; } = customizations.Value;
	private FileSystemService FileSystemService { get; } = fileSystemService;
	private EmailTemplateService TemplateService { get; } = templateService;

	public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
		SendEmailAsync(email, "Confirm your email",
			$"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

	public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
		SendEmailAsync(email, "Reset your password",
			$"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

	public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
		SendEmailAsync(email, "Reset your password",
			$"Please reset your password using the following code: {resetCode}");


	public Task SendEmailAsync(string email, string subject, string htmlMessage) {
		return SendEmailAsync(email, null, subject, htmlMessage);
	}

	public Task SendEmailAsync(string email, string? name, string subject, string htmlMessage)
		=> SendEmailAsync(email, name, subject, htmlMessage, []);
	public async Task SendEmailAsync(string email, string? name, string subject, string htmlMessage, params Header[] header) {
		try {
			var message = new MimeMessage {
				From = {new MailboxAddress(Configuration.SenderName, Configuration.SenderEmail)},
				To = { new MailboxAddress(name, email) },
				Subject = subject
			};

			var builder = new BodyBuilder {
				HtmlBody = htmlMessage
			};

			message.Body = builder.ToMessageBody();
			foreach (var h in header) {
				message.Headers.Add(h);
			}

			using var client = new SmtpClient();
			await client.ConnectAsync(Configuration.Host, Configuration.Port, 
				Configuration.Ssl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None);
			if (!string.IsNullOrWhiteSpace(Configuration.Username)) {
				await client.AuthenticateAsync(Configuration.Username, Configuration.Password);
			}

			try {
				await client.SendAsync(message);
			} catch (Exception ex) {
				throw new EmailNotSendException("Failed Email send.", ex);
			}
			await client.DisconnectAsync(true);
			Logger.LogInformation("Successfully send mail to {email} (subject: {subject}).", email, subject);
		} catch (Exception ex) {
			Logger.LogError(ex, "Error sending E-Mail");
			throw;
		}
	}

	public Task SendDefaultMailAsync(string receiverMail, string? receiverName, string subject, string title, string bodyHtml) {
		var host = new Uri(string.IsNullOrWhiteSpace(Customizations.AppUrl) ? "" : Customizations.AppUrl); // TODO get link
		string logo = !string.IsNullOrWhiteSpace(Customizations.LogoLink)
			? Customizations.LogoLink
			: new Uri(host, "/img/logo.png").AbsoluteUri;
		string body = TemplateService.Default(host.AbsoluteUri, logo, title, bodyHtml);
		return SendEmailAsync(receiverMail, receiverName, subject, body);
	}

	public async Task SendSubscribedMailAsync(EmailSubscriber subscriber, string subject, string title, string bodyHtml, 
			string browserUrl = "", string subscribedRole = "-1") {
		(string user, string token) = await TemplateService
			.CreateConfirmTokensAsync(subscriber.Id, "unsubscribe-"+subscribedRole, TimeSpan.FromDays(30));
		var host = new Uri(string.IsNullOrWhiteSpace(Customizations.AppUrl) ? "" : Customizations.AppUrl); // TODO get link
		browserUrl = string.IsNullOrWhiteSpace(browserUrl) ? host.AbsoluteUri : browserUrl; // TODO find better solution
		
		string logo = !string.IsNullOrWhiteSpace(Customizations.LogoLink)
			? Customizations.LogoLink
			: new Uri(host, "/img/logo.png").AbsoluteUri;
		string unsubscribeLink = new Uri(host, 
			$"/Email/Unsubscribe?newsletter={subscribedRole}&user={WebUtility.UrlEncode(user)}&token={WebUtility.UrlEncode(token)}").AbsoluteUri;
		string body = TemplateService.Newsletter(host.AbsoluteUri, browserUrl, logo, title, bodyHtml, unsubscribeLink);
		await SendEmailAsync(subscriber.Email, subscriber.Name, subject, body, 
			new Header(HeaderId.ListUnsubscribe, $"<{unsubscribeLink}>"),
			new Header(HeaderId.ListUnsubscribePost, "One-Click"));
	}

	public async Task SendWelcomeMailAsync(EmailSubscriber subscriber, string subject, string title, string bodyHtml, 
		IEnumerable<EmailNewsletter> articles) {
		(string user, string token) = await TemplateService
			.CreateConfirmTokensAsync(subscriber.Id, "unsubscribe-welcome", TimeSpan.FromDays(30));
		var host = new Uri(string.IsNullOrWhiteSpace(Customizations.AppUrl) ? "" : Customizations.AppUrl); // TODO get link

		string articlePartial = (await FileSystemService.GetPartialTemplateAsync("email-article", """
			<div style="padding: 10px; background: #9f9f9f; color: #fff; margin-bottom: 10px; border-radius: 2px">
			  <h3 style="margin-top: 0;">{0}</h3>
			  <small>{1}</small>
			  <p>{2}...</p>
			  <a href="{3}">Link</a>
			</div>
			"""))!;
		var articlesHtml = new StringBuilder("");
		foreach (var n in articles) {
			string articleLink = ArticleUtilities.GenerateArticleLink(n.Article, new Uri(Customizations.AppUrl, UriKind.Absolute));
			articlesHtml.AppendFormat(
				articlePartial, 
				n.Article.Title, n.Article.Author.Name, n.Article.Body[..Math.Min(250, n.Article.Body.Length)], articleLink);
		}

		string logo = !string.IsNullOrWhiteSpace(Customizations.LogoLink)
			? Customizations.LogoLink
			: new Uri(host, "/img/logo.png").AbsoluteUri;
		string unsubscribeLink = new Uri(host, 
			$"/Email/Unsubscribe?newsletter=welcome&user={WebUtility.UrlEncode(user)}&token={WebUtility.UrlEncode(token)}").AbsoluteUri;
		string body = TemplateService.Welcome(host.AbsoluteUri, logo, title, bodyHtml, unsubscribeLink, articlesHtml.ToString());
		await SendEmailAsync(subscriber.Email, subscriber.Name, subject, body, 
			new Header(HeaderId.ListUnsubscribe, $"<{unsubscribeLink}>"),
			new Header(HeaderId.ListUnsubscribePost, "One-Click"));
	}
}

public class EmailNotSendException(string message, Exception exception) : ApplicationException(message, exception);