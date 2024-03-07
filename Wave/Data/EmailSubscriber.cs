using System.ComponentModel.DataAnnotations;

namespace Wave.Data;

public class EmailSubscriber {
	[Key]
	public Guid Id { get; set; }

	[MaxLength(128)]
	public string? Name { get; set; }
	[EmailAddress, MaxLength(256)]
	public required string Email { get; set; }
	[MaxLength(8)]
	public required string Language { get; set; } = "en-US";

	[MaxLength(256)]
	public string? UnsubscribeReason { get; set; }
	public DateTimeOffset? LastMailReceived { get; set; }
	public DateTimeOffset? LastMailOpened { get; set; }

	public bool Unsubscribed { get; set; }
}