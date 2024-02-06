using System.Text;
using System.Xml;
using System.Xml.Linq;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Wave.Data;
using static StackExchange.Redis.Role;

namespace Wave.Controllers;

[ApiController]
public class SitemapController(ApplicationDbContext context) : ControllerBase {
	private ApplicationDbContext Context { get; } = context;

	[HttpGet("/sitemap.xml")]
	[Produces("application/xml")]
	[ResponseCache(Duration = 60*15, Location = ResponseCacheLocation.Any)]
	[OutputCache(Duration = 60*15)]
	public async Task GetSitemapAsync(CancellationToken cancellation) {
		var host = new Uri($"https://{Request.Host}{Request.PathBase}", UriKind.Absolute);
		var articles = await Context.Set<Article>().OrderBy(a => a.PublishDate).ToListAsync();

		var document = new XDocument {
			Declaration = new XDeclaration("1.0", Encoding.UTF8.ToString(), null),
		};
		
		XNamespace nameSpace = "http://www.sitemaps.org/schemas/sitemap/0.9";
		var root = new XElement(nameSpace + "urlset");
		if (articles.Count > 0) {
			root.Add(CreateUrlElement(nameSpace, host, articles.Max(a => a.PublishDate).UtcDateTime, priority:1f));

			foreach (var article in articles) {
				root.Add(CreateUrlElement(nameSpace, new Uri(host,
					$"/{article.PublishDate.Year}/{article.PublishDate.Month:D2}/{article.PublishDate.Day:D2}/{Uri.EscapeDataString(article.Title.ToLowerInvariant()).Replace("-", "+").Replace("%20", "-")}"), article.LastModified?.UtcDateTime ?? article.PublishDate.UtcDateTime));
			}
		} else {
			root.Add(CreateUrlElement(nameSpace, host, priority:1f));
		}
		document.Add(root);
		
		Response.StatusCode = StatusCodes.Status200OK;
		Response.ContentType = "application/xml; charset=utf-8";
		await using var writer = XmlWriter.Create(Response.Body, new XmlWriterSettings {
			Encoding = Encoding.UTF8,
			Async = true
		});
		await document.SaveAsync(writer, cancellation);
		await writer.FlushAsync();
	}

	private static XElement CreateUrlElement(XNamespace nameSpace, Uri location, DateTime? lastModified = null, ChangeFrequencies changeFrequency = ChangeFrequencies.Unknown, float priority = 0.5f) {
		var result = new XElement(nameSpace + "url", new XElement(nameSpace + "loc", location.AbsoluteUri));

		if (lastModified is not null) result.Add(new XElement(nameSpace + "lastmod", lastModified.Value.ToString("yyyy-MM-dd")));
		if (changeFrequency is not ChangeFrequencies.Unknown) result.Add(new XElement(nameSpace + "changefreq", changeFrequency.ToString().ToLower()));
		if (Math.Abs(priority - 0.5f) > 0.05) result.Add(new XElement(nameSpace + "priority", priority.ToString("F1")));

		return result;
	}

	private enum ChangeFrequencies {
		Unknown,
		Never,
		Always,
		Hourly,
		Daily,
		Weekly,
		Monthly,
		Yearly
	}
}