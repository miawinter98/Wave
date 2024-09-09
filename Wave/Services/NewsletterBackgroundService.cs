using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wave.Data;
using Wave.Utilities;

namespace Wave.Services;

public class NewsletterBackgroundService(ILogger<NewsletterBackgroundService> logger, IDbContextFactory<ApplicationDbContext> contextFactory, IServiceProvider serviceProvider, IOptions<Customization> customizations) : IScopedProcessingService {
	private ILogger<NewsletterBackgroundService> Logger { get; } = logger;
	private IDbContextFactory<ApplicationDbContext> ContextFactory { get; } = contextFactory;
	private IServiceProvider ServiceProvider { get; } = serviceProvider;
	private Customization Customizations { get; } = customizations.Value;

	public async ValueTask DoWork(CancellationToken cancellationToken) {
		try {
			Logger.LogDebug("Checking Articles...");

			await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);
			var now = DateTimeOffset.UtcNow;
			var newsletters = context.Set<EmailNewsletter>()
				.IgnoreQueryFilters()
				.Include(n => n.Article.Author)
				.Include(n => n.Article.Categories)
				.Where(n => !n.Article.IsDeleted && !n.IsSend && n.DistributionDateTime <= now)
				.ToList();
			if (newsletters.Count < 1) return;

			Logger.LogInformation("Processing {count} Articles...", newsletters.Count);
			
			await using var client = ServiceProvider.GetRequiredKeyedService<IEmailService>("bulk");
			await client.ConnectAsync(cancellationToken);
			var factory = ServiceProvider.GetRequiredService<EmailFactory>();
			
			foreach (var newsletter in newsletters) {
				if (cancellationToken.IsCancellationRequested) {
					Logger.LogInformation("Cancellation requested, skipping processing '{title}'.", newsletter.Article.Title);
					return;
				}
				string replyTo = "";
				if (!string.IsNullOrWhiteSpace(newsletter.Article.Author.ContactEmail))
					replyTo = $"{newsletter.Article.Author.Name} <{newsletter.Article.Author.ContactEmail}>";
				string aboutTheAuthor = await factory.CreateAuthorCard(
					newsletter.Article.Author, 
					new Uri(Customizations.AppUrl, UriKind.Absolute));

				Logger.LogInformation("Processing '{title}'.", newsletter.Article.Title);
				// set newsletter to send first, so we don't spam people 
				// in case something unforeseen goes wrong
				newsletter.IsSend = true;
				await context.SaveChangesAsync(cancellationToken);
				string articleLink = ArticleUtilities.GenerateArticleLink(newsletter.Article, new Uri(Customizations.AppUrl, UriKind.Absolute));
				
				EmailSubscriber? last = null;
				while (context.Set<EmailSubscriber>()
							.Where(s => (last == null || s.Id > last.Id))
							.OrderBy(s => s.Id)
							.Take(50)
							.ToList() is { Count: > 0 } subscribers) {
					last = subscribers.Last();

					foreach (var subscriber in subscribers) {
						var email = await factory.CreateSubscribedEmail(
							subscriber, articleLink, 
							newsletter.Article.Title,
							newsletter.Article.Title,
							newsletter.Article.BodyHtml + aboutTheAuthor, 
							newsletter.Article.BodyPlain, 
							"newsletter-" + newsletter.Id, replyTo);
						await client.SendEmailAsync(email, cancellationToken);
					}
				}
			}

			Logger.LogInformation("Processing complete.");
		} catch (Exception ex) {
			Logger.LogError(ex, "Failed to distribute emails.");
		}
	}
}