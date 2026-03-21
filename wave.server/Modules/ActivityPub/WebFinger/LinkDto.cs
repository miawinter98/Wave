namespace wave.server.Modules.ActivityPub.WebFinger;

public record LinkDto(string Rel, string Type, Uri? Href, string? Template);
