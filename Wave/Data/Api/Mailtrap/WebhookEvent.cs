using System.Text.Json.Serialization;

namespace Wave.Data.Api.Mailtrap;

public enum WebhookEventType {
	Delivery,
	SoftBounce,
	Bounce,
	Suspension,
	Unsubscribe,
	Open,
	SpamComplaint,
	Click,
	Reject
}

public record WebhookEvent(
	[property:JsonPropertyName("event")] 
	string EventTypeString,
	[property:JsonPropertyName("category")] 
	string Category,
	[property:JsonPropertyName("message_id")] 
	string MessageId,
	[property:JsonPropertyName("event_id")] 
	string EventId,
	[property:JsonPropertyName("email")] 
	string Email,
	[property:JsonPropertyName("timestamp")] 
	long Timestamp,
	[property:JsonPropertyName("response")] 
	string? Response,
	[property:JsonPropertyName("reason")] 
	string? Reason,
	[property:JsonPropertyName("response_code")] 
	int? ResponseCode) {

	public WebhookEventType Type => Enum.Parse<WebhookEventType>(EventTypeString.Replace("_", ""), true);
	public DateTimeOffset EventDateTime => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp);
}

public record Webhook {
	[JsonPropertyName("events")] 
	[JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
	public ICollection<WebhookEvent> Events { get; } = [];

	public override string ToString() {
		return $"Webhook {{ Events = [{string.Join(", ", Events)}] }}";
	}
}