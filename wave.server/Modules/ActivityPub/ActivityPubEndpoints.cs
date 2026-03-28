using wave.server.Modules.ActivityPub.Actor;
using wave.server.Modules.ActivityPub.WebFinger;

namespace wave.server.Modules.ActivityPub;

public static class ActivityPubEndpoints
{
	public static void MapActivityPubEndpoints(this IEndpointRouteBuilder route)
	{
		route.MapWellKnownEndpoints();
		route.MapActorEndpoints();
	}
}
