using System.ComponentModel;
using System.Text.Json.Serialization;

namespace wave.server.Modules.ActivityPub.Models;

public record AttachmentDto(string Name, string Value)
{
	public string Type { get; } = "PropertyValue";
}

public record PersonDto(
	Uri? Id,
	string? Name,
	string? Summary,
	DateTimeOffset Published,
	Uri? Inbox,
	Uri? Outbox,
	Uri? Following,
	Uri? Followers,
	[property: Description("Wave profile URL")] Uri? Url,
	string? PreferredUsername,
	bool ManuallyApprovesFollowers,
	bool Discoverable,
	bool Indexable,
	ImageDto? Image,
	ImageDto? Icon,
	PublicKeyRecord PublicKey,
	IReadOnlyList<AttachmentDto>? Attachment = null
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
	public string Type { get; } = "Person";
	public bool Memorial { get; } = false;

	[JsonPropertyName("@context"), JsonPropertyOrder(-1)]
	public object[] Context { get; } =
	[
		"https://www.w3.org/ns/activitystreams",
		"https://w3id.org/security/v1",
		new Dictionary<string, string>()
		{
			{ "manuallyApprovesFollowers", "as:manuallyApprovesFollowers" },
			{ "sensitive", "as:sensitive" },
			{ "discoverable", "toot:discoverable" },
			{ "schema", "http://schema.org#" },
			{ "PropertyValue", "schema:PropertyValue" },
			{ "value", "schema:value" },
			{ "toot", "http://joinmastodon.org/ns#" },
			{ "memorial", "toot:memorial" },
			{ "indexable", "toot:indexable" },
		},
	];
}
