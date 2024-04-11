using System.Net;
using System.ServiceModel.Syndication;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wave.Data;
using Wave.Utilities;
using Wave.Utilities.Metrics;

namespace Wave.Controllers;

[ApiController]
[Route("/[controller]")]
public class RssController(IOptions<Customization> customizations, ApplicationDbContext context, IOptions<Features> features, RssMetrics metrics) : ControllerBase {
	private ApplicationDbContext Context { get; } = context;
	private IOptions<Customization> Customizations { get; } = customizations;
	private IOptions<Features> Features { get; } = features;

	[HttpGet("rss.xml", Name = "RssFeed")]
	[Produces("application/rss+xml")]
	[ResponseCache(Duration = 60*15, Location = ResponseCacheLocation.Any)]
	public async Task<IActionResult> GetRssFeedAsync(string? category = null, Guid? author = null) {
		if (!Features.Value.Rss) return new JsonResult("RSS is disabled") {StatusCode = StatusCodes.Status401Unauthorized};

		metrics.RssRequestReceived("application/rss+xml", category, author?.ToString());

		if (category is not null || author.HasValue)
			Response.Headers.Append("x-robots-tag", "noindex");

		var feed = await CreateFeedAll("RssFeed", category, author);
		if (feed is null) return NotFound();
		Response.ContentType = "application/atom+xml";
		return Ok(feed);
	}
	[HttpGet("atom.xml", Name = "AtomFeed")]
	[Produces("application/atom+xml")]
	[ResponseCache(Duration = 60*15, Location = ResponseCacheLocation.Any)]
	public async Task<IActionResult> GetAtomFeedAsync(string? category = null, Guid? author = null) {
		if (!Features.Value.Rss) return new JsonResult("RSS is disabled") {StatusCode = StatusCodes.Status401Unauthorized};
		
		metrics.RssRequestReceived("application/atom+xml", category, author?.ToString());

		if (category is not null || author.HasValue)
			Response.Headers.Append("x-robots-tag", "noindex");

		var feed = await CreateFeedAll("AtomFeed", category, author);
		if (feed is null) return NotFound();
		Response.ContentType = "application/atom+xml";
		return Ok(feed);
	}
	
	private async Task<SyndicationFeed?> CreateFeedAll(string? routeName, string? category, Guid? author) {
		var now = DateTimeOffset.UtcNow;
		IQueryable<Article> query = Context.Set<Article>()
			.Include(a => a.Author)
			.Include(a => a.Categories)
			.Where(a => a.Status >= ArticleStatus.Published && a.PublishDate <= now)
			.OrderByDescending(a => a.PublishDate);

		if (!string.IsNullOrWhiteSpace(category)) {
			query = query.Where(a => a.Categories.Any(c => c.Name == category));
		}
		if (author is { } a1) {
			string authorString = a1.ToString();
			query = query.Where(a => a.Author.Id == authorString);
		}

		query = query.Take(15);
		var articles = await query.ToListAsync();
		if (articles.Count < 1) return null;
		var date = query.Max(a => a.PublishDate);

		return CreateFeedAsync(articles, date, routeName, category, author);
	}

	private SyndicationFeed CreateFeedAsync(IEnumerable<Article> articles, DateTimeOffset date, 
			string? routeName, string? category, Guid? author) {
		var customizations = Customizations.Value;

		string appName = customizations.AppName;
		Uri host;
		if (string.IsNullOrWhiteSpace(customizations.AppUrl)) {
			host = new Uri(customizations.AppUrl);
		} else {
			host = new Uri($"https://{Request.Host}", UriKind.Absolute);
		}
		var feedLink = new UriBuilder(Url.RouteUrl(routeName, null, "https", host.Host) ?? host.AbsoluteUri);
		var htmlLink = new UriBuilder(host);
		if (category is not null) {
			feedLink.Query = "category=" + WebUtility.HtmlEncode(category);
			htmlLink.Path = "/category/" + WebUtility.HtmlEncode(category);
		}
		if (author is not null) {
			var query = HttpUtility.ParseQueryString(feedLink.Query);
			query.Add("author", author.ToString());
			feedLink.Query = query.ToString();
			
			if (htmlLink.Path.Length < 2) {
				htmlLink.Path = "/profile/" + author;
			} else {
				htmlLink.Query = "?author=" + author;
			}
		}
		
		var feed = new SyndicationFeed(appName, "Feed on " + appName, htmlLink.Uri, host.AbsoluteUri, date) {
			TimeToLive = TimeSpan.FromMinutes(15),
			Generator = "Wave",
			Links = { new SyndicationLink(feedLink.Uri) {RelationshipType = "self"} },
			Items = GetItems(articles, host)
		};
		if (category != null) feed.Categories.Add(new SyndicationCategory(category));

		return feed;
	}

	private static IEnumerable<SyndicationItem> GetItems(IEnumerable<Article> articles, Uri host) {
		return articles.Select(article => {
				var item = new SyndicationItem(
					article.Title,
					new TextSyndicationContent(article.BodyHtml, TextSyndicationContentKind.Html),
					new Uri(ArticleUtilities.GenerateArticleLink(article, host)),
					new Uri(host, "article/" + article.Id).AbsoluteUri,
					article.LastPublicChange) 
				{
					Authors = {
						new SyndicationPerson {Name = article.Author.FullName}
					},
					LastUpdatedTime = article.LastModified ?? article.PublishDate,
					PublishDate = article.PublishDate
				};

				foreach (var category in article.Categories.OrderBy(c => c.Color)) {
					item.Categories.Add(new SyndicationCategory(category.Name));
				}

				return item;
			})
			.ToList();
	}
}