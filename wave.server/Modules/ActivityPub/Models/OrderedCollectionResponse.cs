namespace wave.server.Modules.ActivityPub.Models;

public record OrderedCollectionResponse<T>(
	string? Summary,
	int TotalItems,
	IReadOnlyCollection<T>? OrderedItems,
	Uri? First
);
