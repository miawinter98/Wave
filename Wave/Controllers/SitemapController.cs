using System.Globalization;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wave.Data;
using Wave.Utilities;

namespace Wave.Controllers;

[ApiController]
public class SitemapController(ApplicationDbContext context, IOptions<Features> features) : ControllerBase {
	private ApplicationDbContext Context { get; } = context;
	private Features Features { get; } = features.Value;

	[HttpGet("/sitemap.xml")]
	[Produces("application/xml")]
	[ResponseCache(Duration = 60*15, Location = ResponseCacheLocation.Any)]
	[OutputCache(Duration = 60*15)]
	public async Task GetSitemapAsync(CancellationToken cancellation) {
		var host = new Uri($"https://{Request.Host}{Request.PathBase}", UriKind.Absolute);
		var articles = await Context.Set<Article>().OrderBy(a => a.PublishDate).ToListAsync(cancellation);
		var categories = await Context.Set<Category>().Where(c => c.Articles.Any())
			.OrderBy(c => c.Color).ThenBy(c => c.Id)
			.Select(c => new {c.Name, LastModified = c.Articles.Max(a => a.PublishDate)}).ToListAsync(cancellation);
		var profiles = await Context.Set<ApplicationUser>().Where(a => a.Articles.Any())
			.OrderBy(a => a.Id)
			.Select(a => new {a.Id, LastModified = a.Articles.Max(ar => ar.PublishDate)})
			.ToListAsync(cancellation);

		var document = new XDocument {
			Declaration = new XDeclaration("1.0", Encoding.UTF8.ToString(), null),
		};
		
		XNamespace nameSpace = "http://www.sitemaps.org/schemas/sitemap/0.9";
		var root = new XElement(nameSpace + "urlset");
		if (articles.Count > 0) {
			root.Add(CreateUrlElement(nameSpace, host, articles.Max(a => a.PublishDate).UtcDateTime, ChangeFrequencies.Daily, 1.0f));

			foreach (var article in articles) {
				root.Add(CreateUrlElement(nameSpace, 
					new Uri(ArticleUtilities.GenerateArticleLink(article, host)), 
					article.LastPublicChange.UtcDateTime, priority:0.8f));
			}
		} else {
			root.Add(CreateUrlElement(nameSpace, host, DateTimeOffset.Now.UtcDateTime, ChangeFrequencies.Daily, 1.0f));
		}

		foreach (var category in categories) {
			root.Add(CreateUrlElement(nameSpace, 
				new Uri(host, "/category/" + WebUtility.UrlEncode(category.Name)), 
				category.LastModified.UtcDateTime, priority:0.2f));
		}

		foreach (var profile in profiles) {
			root.Add(CreateUrlElement(nameSpace, 
				new Uri(host, "/profile/" + profile.Id),
				profile.LastModified.UtcDateTime, ChangeFrequencies.Weekly, 0.6f));
		}

		root.Add(CreateUrlElement(nameSpace, new Uri(host, "/Account/Login")));
		if (Features.NativeSignup) root.Add(CreateUrlElement(nameSpace, new Uri(host, "/Account/Register")));
		if (Features.EmailSubscriptions) root.Add(CreateUrlElement(nameSpace, new Uri(host, "/Email/Subscribe")));

		document.Add(root);
		
		Response.StatusCode = StatusCodes.Status200OK;
		Response.ContentType = "application/xml; charset=utf-8";
		await using var writer = XmlWriter.Create(Response.Body, new XmlWriterSettings {
			Encoding = Encoding.UTF8,
			Async = true, Indent = true
		});
		await document.SaveAsync(writer, cancellation);
		await writer.FlushAsync();
	}

	private static XElement CreateUrlElement(XNamespace nameSpace, Uri location, DateTime? lastModified = null, ChangeFrequencies changeFrequency = ChangeFrequencies.Unknown, float priority = 0.5f) {
		var result = new XElement(nameSpace + "url", new XElement(nameSpace + "loc", location.AbsoluteUri));

		if (lastModified is not null) result.Add(new XElement(nameSpace + "lastmod", lastModified.Value.ToString("yyyy-MM-dd")));
		if (changeFrequency is not ChangeFrequencies.Unknown) result.Add(new XElement(nameSpace + "changefreq", changeFrequency.ToString().ToLower()));
		if (Math.Abs(priority - 0.5f) > 0.05) result.Add(new XElement(nameSpace + "priority", priority.ToString("F1", CultureInfo.InvariantCulture)));

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