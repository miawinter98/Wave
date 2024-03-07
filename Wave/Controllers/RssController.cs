using System.ServiceModel.Syndication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wave.Data;
using Wave.Utilities;

namespace Wave.Controllers;

[ApiController]
[Route("/[controller]")]
public class RssController(IOptions<Customization> customizations, ApplicationDbContext context, IOptions<Features> features) : ControllerBase {
	private ApplicationDbContext Context { get; } = context;
	private IOptions<Customization> Customizations { get; } = customizations;
	private IOptions<Features> Features { get; } = features;

	[HttpGet("rss.xml", Name = "RssFeed")]
	[Produces("application/rss+xml")]
	[ResponseCache(Duration = 60*15, Location = ResponseCacheLocation.Any)]
	public async Task<IActionResult> GetRssFeedAsync(string? category = null) {
		if (!Features.Value.Rss) return new JsonResult("RSS is disabled") {StatusCode = StatusCodes.Status401Unauthorized};

		var feed = await CreateFeedAll("RssFeed", category);
		if (feed is null) return NotFound();
		Response.ContentType = "application/atom+xml";
		return Ok(feed);
	}
	[HttpGet("atom.xml", Name = "AtomFeed")]
	[Produces("application/atom+xml")]
	[ResponseCache(Duration = 60*15, Location = ResponseCacheLocation.Any)]
	public async Task<IActionResult> GetAtomFeedAsync(string? category = null) {
		if (!Features.Value.Rss) return new JsonResult("RSS is disabled") {StatusCode = StatusCodes.Status401Unauthorized};

		var feed = await CreateFeedAll("AtomFeed", category);
		if (feed is null) return NotFound();
		Response.ContentType = "application/atom+xml";
		return Ok(feed);
	}
	
	private async Task<SyndicationFeed?> CreateFeedAll(string? routeName, string? category) {
		var now = DateTimeOffset.UtcNow;
		IQueryable<Article> query = Context.Set<Article>()
			.Include(a => a.Author)
			.Include(a => a.Categories)
			.Where(a => a.Status >= ArticleStatus.Published && a.PublishDate <= now)
			.OrderByDescending(a => a.PublishDate);

		if (!string.IsNullOrWhiteSpace(category)) {
			query = query.Where(a => a.Categories.Any(c => c.Name == category));
		}

		query = query.Take(15);
		var articles = await query.ToListAsync();
		if (articles.Count < 1) return null;
		var date = query.Max(a => a.PublishDate);

		return CreateFeedAsync(articles, date, routeName, category);
	}

	private SyndicationFeed CreateFeedAsync(IEnumerable<Article> articles, DateTimeOffset date, string? routeName, string? category) {
		string appName = Customizations.Value.AppName;
		var host = new Uri($"https://{Request.Host}{Request.PathBase}", UriKind.Absolute);
		var link = new Uri(Url.RouteUrl(routeName, null, "https", host.Host) ?? host.AbsoluteUri);

		var feed = new SyndicationFeed(appName, "Feed on " + appName, link, articles
			.Select(article => {
				var item = new SyndicationItem(
					article.Title,
					new TextSyndicationContent(article.BodyHtml, TextSyndicationContentKind.Html),
					new Uri(ArticleUtilities.GenerateArticleLink(article, host)),
					new Uri(host, "article/" + article.Id).AbsoluteUri,
					article.PublishDate) {
					Authors = {
						new SyndicationPerson { Name = article.Author.FullName }
					},
					LastUpdatedTime = article.LastModified ?? article.PublishDate,
					PublishDate = article.PublishDate
				};

				foreach (var category in article.Categories.OrderBy(c => c.Color)) {
					item.Categories.Add(new SyndicationCategory(category.Name));
				}
				return item;
			})
			.ToList()) {
				TimeToLive = TimeSpan.FromMinutes(15),
				LastUpdatedTime = date,
				Generator = "Wave",
				Links = {
					new SyndicationLink(link) { RelationshipType = "self" }
				}
		};
		if (category != null) feed.Categories.Add(new SyndicationCategory(category));

		return feed;
	}
}