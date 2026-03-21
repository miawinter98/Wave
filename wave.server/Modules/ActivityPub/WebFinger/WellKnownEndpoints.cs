using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace wave.server.Modules.ActivityPub.WebFinger;

public static class WellKnownEndpoints
{
	public static void MapWellKnownEndpoints(this IEndpointRouteBuilder route)
	{
		var wellKnow = route.MapGroup(".well-known").WithTags("Well Known");

		wellKnow.MapGet("webfinger", GetAsync);
	}

	public static async Task<Results<Ok<ActorResponse>, NotFound>> GetAsync(
		[FromQuery] string resource,
		WebFingerService service,
		HttpContext context,
		CancellationToken cancellation
	)
	{
		var data = resource.Split(":", 2);

		var resourceType = data[0];
		var resourceName = data[1];

		if (resourceType.Equals("acct", StringComparison.InvariantCultureIgnoreCase))
		{
			var actor = await service.GetActorAsync(
				resourceName,
				context.Request.Scheme,
				cancellation
			);
			return actor is null ? TypedResults.NotFound() : TypedResults.Ok(actor);
		}

		return TypedResults.NotFound();
	}
}
