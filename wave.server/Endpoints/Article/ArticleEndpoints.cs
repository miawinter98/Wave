using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;

namespace wave.server.Endpoints.Article;

public static class ArticleEndpoints
{
    public static void MapArticleEndpoints(this IEndpointRouteBuilder route)
    {
        var article = route.MapGroup("article").WithTags("Articles");

        article
            .MapGet("new", GetNewest)
            .WithName("GetNewestArticle")
            .WithSummary("Get Newest Article");
    }

    private static async Task<Results<Ok<ArticleResponse>, NotFound>> GetNewest(
        ClaimsPrincipal user,
        ArticleService service,
        CancellationToken cancellation
    )
    {
        var result = await service.GetLatestAsync(user, cancellation);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
