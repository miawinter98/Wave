namespace wave.server.Modules.ActivityPub.Models;

public record ImageDto(Uri Url, bool Sensitive = false, string? Name = null)
{
	public string Type { get; } = "Image";
}
