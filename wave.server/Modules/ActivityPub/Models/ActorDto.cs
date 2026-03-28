namespace wave.server.Modules.ActivityPub.Models;

public record ActorDto(
	Uri? Id,
	Uri? Inbox,
	Uri? Outbox,
	Uri? Following,
	Uri? Followers,
	string? PreferredUsername,
	bool ManuallyApprovesFollowers,
	bool Discoverable,
	PublicKeyRecord PublicKey
);
