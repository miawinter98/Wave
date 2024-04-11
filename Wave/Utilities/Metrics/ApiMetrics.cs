using System.Diagnostics.Metrics;

namespace Wave.Utilities.Metrics;

public class ApiMetrics {
	private Counter<int> WebhookEventCounter { get; } 
	private Counter<int> WebhookErrorCounter { get; } 

	public ApiMetrics(IMeterFactory meterFactory) {
		var meter = meterFactory.Create("Wave.Api");

		WebhookEventCounter = meter.CreateCounter<int>("wave.webhook.events", "{events}",
			description: "Counts the incoming webhook events");
		WebhookErrorCounter = meter.CreateCounter<int>("wave.webhook.errors", "{events}",
			description: "Counts errors in webhook events");
	}

	public void WebhookEventReceived(string api, string type) {
		WebhookEventCounter.Add(1, 
			new KeyValuePair<string, object?>("wave.webhook.event_type", type),
			new KeyValuePair<string, object?>("wave.webhook.api", api));
	}

	public void WebhookEventError(string api, string type, string reason) {
		WebhookErrorCounter.Add(1,
			new KeyValuePair<string, object?>("wave.webhook.event_type", type),
			new KeyValuePair<string, object?>("wave.webhook.api", api),
			new KeyValuePair<string, object?>("wave.error.reason", reason));
	}
}