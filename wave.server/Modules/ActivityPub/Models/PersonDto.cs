using System.ComponentModel;

namespace wave.server.Modules.ActivityPub.Models;

public record PersonDto(
	Uri? Id,
	Uri? Inbox,
	Uri? Outbox,
	Uri? Following,
	Uri? Followers,
	[property: Description("Wave profile URL")] Uri? Url,
	string? PreferredUsername,
	bool ManuallyApprovesFollowers,
	bool Discoverable,
	ImageDto? Image,
	ImageDto? Icon,
	PublicKeyRecord PublicKey
)
	: ActorDto(
		Id,
		Inbox,
		Outbox,
		Following,
		Followers,
		PreferredUsername,
		ManuallyApprovesFollowers,
		Discoverable,
		PublicKey
	)
{
	public string Type { get; } = "person";
}
