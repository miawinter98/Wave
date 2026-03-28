using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using wave.server.Modules.ActivityPub.Models;

namespace wave.server.Modules.ActivityPub.Actor;

public static class ActorEndpoints
{
	private const string ActivityJsonTypeName = "application/activity+json";

	public static void MapActorEndpoints(this IEndpointRouteBuilder route)
	{
		var actor = route.MapGroup("activityPub").WithTags("Activity Pub");

		actor
			.MapGet("{actor}", GetActor)
			.Produces<ActorDto>(contentType: ActivityJsonTypeName)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithSummary("Get Actor")
			.WithDescription("Special key 'actor' returns the main actor");
		actor
			.MapGet("{actor}/outbox", GetOutboxAsync)
			.Produces<object>(contentType: "application/activity+json");
		actor
			.MapPost("{actor}/inbox", PostInboxAsync)
			.Accepts<object>(contentType: "application/activity+json")
			.Produces<object>(
				StatusCodes.Status202Accepted,
				contentType: "application/activity+json"
			)
			.ProducesProblem(StatusCodes.Status405MethodNotAllowed);
	}

	public static async Task<Results<JsonHttpResult<ActorDto>, NotFound>> GetActor(
		string actor,
		ActorService service,
		HttpContext context,
		CancellationToken cancellation
	)
	{
		if (actor.Equals("actor", StringComparison.InvariantCultureIgnoreCase))
		{
			return TypedResults.Json<ActorDto>(
				await service.GetMainActorAsync(context.Request.Scheme, cancellation),
				contentType: context.Response.ContentType = ActivityJsonTypeName,
				statusCode: StatusCodes.Status200OK
			);
		}

		var result = await service.GetActorAsync(actor, context.Request.Scheme, cancellation);

		return result is null
			? TypedResults.NotFound()
			: TypedResults.Json<ActorDto>(
				result,
				contentType: context.Response.ContentType = ActivityJsonTypeName,
				statusCode: StatusCodes.Status200OK
			);
	}

	public static async Task<
		Results<JsonHttpResult<OrderedCollectionResponse<object>>, NoContent, NotFound>
	> GetOutboxAsync(
		ClaimsPrincipal principal,
		string actor,
		ActorService service,
		HttpContext context,
		CancellationToken cancellation
	)
	{
		if (actor.Equals("actor", StringComparison.InvariantCultureIgnoreCase))
		{
			return TypedResults.NoContent();
		}

		if (await service.GetOutboxAsync(actor, context.Request.Scheme, cancellation) is { } outbox)
		{
			if (outbox.TotalItems is 0)
				return TypedResults.NoContent();

			return TypedResults.Json(
				outbox,
				contentType: context.Response.ContentType = ActivityJsonTypeName,
				statusCode: StatusCodes.Status200OK
			);
		}

		return TypedResults.NotFound();
	}

	public static async Task<Results<Accepted, StatusCodeHttpResult>> PostInboxAsync(
		ClaimsPrincipal principal,
		string actor,
		CancellationToken cancellation
	)
	{
		return TypedResults.StatusCode(StatusCodes.Status405MethodNotAllowed);
	}
}
