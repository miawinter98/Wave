using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Wave.Data;
using Wave.Data.Migrations.postgres;

namespace Wave.Controllers;


[ApiController]
[Route("/[controller]")]
public class RssController(IOptions<Customization> customizations, ApplicationDbContext context) : ControllerBase {
	private ApplicationDbContext Context { get; } = context;
	private IOptions<Customization> Customizations { get; } = customizations;

	[HttpGet("rss.xml", Name = "RssFeed")]
	[Produces("application/rss+xml")]
	[ResponseCache(Duration = 60*15, Location = ResponseCacheLocation.Any)]
	public async Task<IActionResult> GetRssFeedAsync() {
		return Ok(await CreateFeedAll("RssFeed"));
	}
	[HttpGet("atom.xml", Name = "AtomFeed")]
	[Produces("application/atom+xml")]
	[ResponseCache(Duration = 60*15, Location = ResponseCacheLocation.Any)]
	public async Task<IActionResult> GetAtomFeedAsync() {
		return Ok(await CreateFeedAll("AtomFeed"));
	}
	
	private async Task<SyndicationFeed> CreateFeedAll(string? routeName) {
		var now = DateTimeOffset.UtcNow;
		var query = Context.Set<Article>()
			.Include(a => a.Author)
			.Include(a => a.Categories)
			.Where(a => a.Status >= ArticleStatus.Published && a.PublishDate <= now)
			.OrderByDescending(a => a.PublishDate)
			.Take(15);
		
		var articles = await query.ToListAsync();
		var date = query.Max(a => a.PublishDate);

		return CreateFeedAsync(articles, date, routeName);
	}

	private SyndicationFeed CreateFeedAsync(IEnumerable<Article> articles, DateTimeOffset date, string? routeName) {
		string appName = Customizations.Value.AppName;
		var host = new Uri($"https://{Request.Host}{Request.PathBase}", UriKind.Absolute);
		var link = new Uri(Url.RouteUrl(routeName, null, "https", host.Host) ?? host.AbsoluteUri);

		return new SyndicationFeed(appName, "Feed on " + appName, link, articles
			.Select(article => {
				var item = new SyndicationItem(
					article.Title,
					new TextSyndicationContent(article.BodyHtml, TextSyndicationContentKind.Html),
					new Uri(host,
						$"/{article.PublishDate.Year}/{article.PublishDate.Month:D2}/{article.PublishDate.Day:D2}/{Uri.EscapeDataString(article.Title.ToLowerInvariant()).Replace("-", "+").Replace("%20", "-")}"),
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
	}
}