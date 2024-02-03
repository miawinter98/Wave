using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace Wave.Utilities;

public class SyndicationFeedFormatter : TextOutputFormatter {

	public SyndicationFeedFormatter() {
		SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/rss+xml"));
		SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/atom+xml"));
		
		SupportedEncodings.Add(Encoding.UTF8);
	}

	protected override bool CanWriteType(Type? type)
		=> typeof(SyndicationFeed).IsAssignableFrom(type);

	public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding) {
		var httpContext = context.HttpContext;
		httpContext.Response.Headers.ContentDisposition = "inline";

		var feed = context.Object as SyndicationFeed;
		
		await using var stringWriter = new StringWriter();
		await using var rssWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings {
			Async = true
		});
		System.ServiceModel.Syndication.SyndicationFeedFormatter formatter;
		if (context.ContentType.Value?.StartsWith("application/rss+xml") is true) {
			formatter = new Rss20FeedFormatter(feed) {
				SerializeExtensionsAsAtom = false
			};
		} else if (context.ContentType.Value?.StartsWith("application/atom+xml") is true) {
			formatter = new Atom10FeedFormatter(feed);
		} else {
			throw new FormatException($"The format {context.ContentType.Value} is not supported.");
		}
		formatter.WriteTo(rssWriter);
		rssWriter.Close();

		await httpContext.Response.WriteAsync(stringWriter.ToString(), selectedEncoding);
	}
}