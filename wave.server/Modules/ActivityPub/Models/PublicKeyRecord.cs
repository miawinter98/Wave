using System.Text.Json.Serialization;

namespace wave.server.Modules.ActivityPub.Models;

public record PublicKeyRecord(Uri Id, Uri Owner, string PublicKeyPem)
{
	[JsonPropertyName("@context")]
	public string Context => "https://w3id.org/security/v1";

	public string Type { get; } = "Key";
}
