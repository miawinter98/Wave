using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using wave.server.Data;

namespace wave.server.Endpoints.User;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder route)
    {
        var user = route.MapGroup("user").WithTags("User");

        user.Map("{id:minlength(1)}", GetUserByIdAsync)
            .WithName("GetUserById")
            .WithSummary("Get User By Id")
            .WithDescription("");
    }

    public static async Task<Results<Ok<UserResponse>, NotFound>> GetUserByIdAsync(
        string id,
        UserManager<ApplicationUser> service,
        CancellationToken cancellationToken
    )
    {
        var user = await service.FindByIdAsync(id);

        return user is null ? TypedResults.NotFound() : TypedResults.Ok(new UserResponse(user));
    }
}
