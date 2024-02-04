﻿using System.Collections.ObjectModel;
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
	public async Task<IActionResult> GetRssFeedAsync(string? category = null) {
		var feed = await CreateFeedAll("RssFeed", category);
		if (feed is null) return NotFound();
		return Ok(feed);
	}
	[HttpGet("atom.xml", Name = "AtomFeed")]
	[Produces("application/atom+xml")]
	[ResponseCache(Duration = 60*15, Location = ResponseCacheLocation.Any)]
	public async Task<IActionResult> GetAtomFeedAsync(string? category = null) {
		var feed = await CreateFeedAll("AtomFeed", category);
		if (feed is null) return NotFound();
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
		if (category != null) feed.Categories.Add(new SyndicationCategory(category));

		return feed;
	}
}