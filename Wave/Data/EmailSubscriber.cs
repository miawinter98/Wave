using System.ComponentModel.DataAnnotations;

namespace Wave.Data;

public class EmailSubscriber {
	[Key]
	public Guid Id { get; set; }

	[MaxLength(128)]
	public string? Name { get; set; }
	[EmailAddress, MaxLength(256)]
	public required string Email { get; set; }

	public bool Unsubscribed { get; set; }
}