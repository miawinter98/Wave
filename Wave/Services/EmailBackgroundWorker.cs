using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using Mjml.Net;
using Wave.Data;
using Wave.Utilities;

namespace Wave.Services;

public class EmailBackgroundWorker(ILogger<EmailBackgroundWorker> logger, IDbContextFactory<ApplicationDbContext> contextFactory, IOptions<SmtpConfiguration> config, IOptions<Customization> customizations, IOptions<Features> features, EmailTemplateService templateService) : IHostedService, IDisposable {
	private ILogger<EmailBackgroundWorker> Logger { get; } = logger;
	private IDbContextFactory<ApplicationDbContext> ContextFactory { get; } = contextFactory;
	private SmtpConfiguration Configuration { get; } = config.Value;
	private Customization Customizations { get; } = customizations.Value;
	private Features Features { get; } = features.Value;
	private EmailTemplateService TemplateService { get; } = templateService;

	private Timer? Timer { get; set; }

	public Task StartAsync(CancellationToken cancellationToken) {
		if (!Features.EmailSubscriptions) return Task.CompletedTask;

		Logger.LogInformation("Background email worker starting.");

		// we want this timer to execute every 15 minutes, at fixed times (:00, :15, :30, :45)
		var now = DateTimeOffset.UtcNow;
		int nowMinute = now.Minute;
		int waitTime = 15 - nowMinute % 15;
		Logger.LogInformation("First distribution check will be in {waitTime} minutes, at {time}.", 
			waitTime, now.AddMinutes(waitTime).LocalDateTime.ToString("u"));
		Timer = new Timer(DoWork, null, TimeSpan.FromMinutes(waitTime), TimeSpan.FromMinutes(15));

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken) {
		if (!Features.EmailSubscriptions) return Task.CompletedTask;

		Logger.LogInformation("Background email worker stopping.");
		Timer?.Change(Timeout.Infinite, 0);
		return Task.CompletedTask;
	}

	public void Dispose() {
		Timer?.Dispose();
		GC.SuppressFinalize(this);
	}

	private void DoWork(object? _) {
		try {
			Logger.LogInformation("Checking Articles...");

			using var context = ContextFactory.CreateDbContext();
			var now = DateTimeOffset.UtcNow;
			var newsletters = context.Set<EmailNewsletter>()
				.Include(n => n.Article.Author)
				.Include(n => n.Article.Categories)
				.Where(n => !n.IsSend && n.DistributionDateTime <= now)
				.ToList();
			if (newsletters.Count < 1) return;

			Logger.LogInformation("Processing {count} Articles...", newsletters.Count);

			var sender = new MailboxAddress(Configuration.SenderName, Configuration.SenderEmail);
			using var client = new SmtpClient();
			client.Connect(Configuration.Host, Configuration.Port,
				Configuration.Ssl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None);
			if (!string.IsNullOrWhiteSpace(Configuration.Username)) {
				client.Authenticate(Configuration.Username, Configuration.Password);
			}

			var mjmlRenderer = new MjmlRenderer();
			var options = new MjmlOptions {
				Beautify = false
			};
			foreach (var newsletter in newsletters) {
				Logger.LogInformation("Processing '{title}'.", newsletter.Article.Title);
				// set newsletter to send first, so we don't spam people 
				// in case something unforeseen goes wrong
				newsletter.IsSend = true;
				context.SaveChanges();

				string articleLink = ArticleUtilities.GenerateArticleLink(
					newsletter.Article, new Uri(Customizations.AppUrl, UriKind.Absolute));
				string unsubscribeLink = new Uri(new Uri(Customizations.AppUrl, UriKind.Absolute), "/unsubscribe").AbsoluteUri;
				string template = TemplateService.Process("newsletter", new Dictionary<EmailTemplateService.Constants, object?>{
					{EmailTemplateService.Constants.BrowserLink, articleLink},
					{EmailTemplateService.Constants.ContentLogo, "https://blog.winter-software.com/img/logo.png"},
					{EmailTemplateService.Constants.ContentTitle, newsletter.Article.Title},
					{EmailTemplateService.Constants.ContentBody, newsletter.Article.BodyHtml},
					{EmailTemplateService.Constants.EmailUnsubscribeLink, unsubscribeLink}
				});

				var message = new MimeMessage {
					From = { sender },
					Subject = newsletter.Article.Title
				};
				var builder = new BodyBuilder {
					HtmlBody = mjmlRenderer.Render(template, options).Html
				};
				message.Body = builder.ToMessageBody();

				EmailSubscriber? last = null;
				while (context.Set<EmailSubscriber>()
							.Where(s => !s.Unsubscribed && (last == null || s.Id > last.Id))
							.OrderBy(s => s.Id)
							.Take(50)
							.ToList() is { Count: > 0 } subscribers) {
					last = subscribers.Last();

					foreach (var subscriber in subscribers) {
						message.To.Clear();
						message.To.Add(new MailboxAddress(subscriber.Name, subscriber.Email));
						client.Send(message);
					}

					Task.Delay(TimeSpan.FromSeconds(10)).Wait();
				}
			}

			client.Disconnect(true);
			Logger.LogInformation("Processing complete.");
		} catch (Exception ex) {
			Logger.LogError(ex, "Failed to distribute emails.");
		}
	}
}