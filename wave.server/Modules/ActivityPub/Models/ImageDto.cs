namespace wave.server.Modules.ActivityPub.Models;

public record ImageDto(
	Uri Url,
	string MediaType = "image/webp",
	bool Sensitive = false,
	string? Name = null
)
{
	public string Type { get; } = "Image";
}
