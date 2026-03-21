namespace wave.server.Modules.ActivityPub.WebFinger;

public record ActorResponse(string Subject, IList<string> Aliases, IList<LinkDto> Links);
