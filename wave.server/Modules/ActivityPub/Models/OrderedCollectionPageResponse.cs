namespace wave.server.Modules.ActivityPub.Models;

public record OrderedCollectionPageResponse<T>(
	string? Summary,
	int TotalItems,
	IReadOnlyCollection<T>? OrderedItems,
	Uri? First,
	Uri? PartOf,
	Uri? PrevUri,
	Uri? Next
) : OrderedCollectionResponse<T>(Summary, TotalItems, OrderedItems, First);
