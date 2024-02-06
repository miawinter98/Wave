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

		var document = new XDocument(new XElement("urlset")) {
			Declaration = new XDeclaration("1.0", Encoding.UTF8.ToString(), null),
		};

		if (articles.Count > 0) {
			document.Root.Add(CreateUrlElement(host, articles.Max(a => a.PublishDate).UtcDateTime, priority:1f));

			foreach (var article in articles) {
				document.Root.Add(CreateUrlElement(new Uri(host,
					$"/{article.PublishDate.Year}/{article.PublishDate.Month:D2}/{article.PublishDate.Day:D2}/{Uri.EscapeDataString(article.Title.ToLowerInvariant()).Replace("-", "+").Replace("%20", "-")}"), article.LastModified?.UtcDateTime ?? article.PublishDate.UtcDateTime));
			}
		} else {
			document.Root.Add(CreateUrlElement(host, priority:1f));
		}
		
		Response.StatusCode = StatusCodes.Status200OK;
		Response.ContentType = "application/xml; charset=utf-8";
		await using var writer = XmlWriter.Create(Response.Body, new XmlWriterSettings {
			Encoding = Encoding.UTF8,
			Async = true
		});
		await document.SaveAsync(writer, cancellation);
		await writer.FlushAsync();
	}

	private static XElement CreateUrlElement(Uri location, DateTime? lastModified = null, ChangeFrequencies changeFrequency = ChangeFrequencies.Unknown, float priority = 0.5f) {
		var result = new XElement("url", new XElement("loc", location.AbsoluteUri));

		if (lastModified is not null) result.Add(new XElement("lastmod", lastModified.Value.ToString("yyyy-MM-dd")));
		if (changeFrequency is not ChangeFrequencies.Unknown) result.Add(new XElement("changefreq", changeFrequency.ToString().ToLower()));
		if (Math.Abs(priority - 0.5f) > 0.05) result.Add(new XElement("priority", priority.ToString("F1")));

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