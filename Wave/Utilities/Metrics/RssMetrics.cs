using System.Diagnostics.Metrics;

namespace Wave.Utilities.Metrics;

public class RssMetrics {
	private Counter<int> RssRequestCounter { get; }

	public RssMetrics(IMeterFactory meterFactory) {
		var meter = meterFactory.Create("Wave.Rss");

		RssRequestCounter = meter.CreateCounter<int>("wave.rss.requests", "{requests}",
			"Counts incoming requests processed on RSS endpoints");
	}

	public void RssRequestReceived(string type, string? category, string? author) {
		RssRequestCounter.Add(1, 
			new KeyValuePair<string, object?>("wave.rss.type", type),
			new KeyValuePair<string, object?>("wave.rss.category", category),
			new KeyValuePair<string, object?>("wave.rss.author", author));
	}
}