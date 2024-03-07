namespace Wave.Data.Api;

public record EmailSubscriberDto(
	string Email, string? Name, string Language, 
	bool Unsubscribed, string? Reason, 
	DateTimeOffset? LastReceived, DateTimeOffset? LastOpened) {

	public EmailSubscriberDto(EmailSubscriber subscriber) 
		: this(subscriber.Email, subscriber.Name, subscriber.Language, 
			subscriber.Unsubscribed, subscriber.UnsubscribeReason, 
			subscriber.LastMailReceived, subscriber.LastMailOpened) {}
}