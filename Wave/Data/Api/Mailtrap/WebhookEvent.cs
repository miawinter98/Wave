using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Wave.Data.Api.Mailtrap;

[JsonConverter(typeof(JsonStringEnumConverter<WebhookEventType>))]
public enum WebhookEventType {
	Delivery,
	[EnumMember(Value = "soft bounce")]
	SoftBounce,
	Bounce,
	Suspension,
	Unsubscribe,
	Open,
	[EnumMember(Value = "spam complaint")]
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

	public WebhookEventType Type => Enum.Parse<WebhookEventType>(EventTypeString.Replace("_", "").Replace(" ", ""), true);
	public DateTimeOffset EventDateTime => DateTimeOffset.FromUnixTimeSeconds(Timestamp);
}

public record Webhook {
	[JsonPropertyName("events")] 
	[JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
	public ICollection<WebhookEvent> Events { get; } = [];

	public override string ToString() {
		return $"Webhook {{ Events = [{string.Join(", ", Events)}] }}";
	}
}