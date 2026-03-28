namespace wave.server.Modules.ActivityPub.Models;

public record ApplicationDto(
	Uri? Id,
	Uri? Inbox,
	Uri? Outbox,
	Uri? Following,
	Uri? Followers,
	string? PreferredUsername,
	bool ManuallyApprovesFollowers,
	bool Discoverable,
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
	);
