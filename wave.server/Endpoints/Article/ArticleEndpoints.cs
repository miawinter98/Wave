using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;

namespace wave.server.Endpoints.Article;

public static class ArticleEndpoints
{
    public static void MapArticleEndpoints(this IEndpointRouteBuilder route)
    {
        var article = route.MapGroup("article").WithTags("Articles");

        article
            .MapGet("latest", GetLatestAsync)
            .WithName("GetLatestArticle")
            .WithSummary("Get Latest Article")
            .WithDescription(
                """
                Returns the article that has most recently been published.
                This endpoint does not check for user access permissions, it only 
                returns the newest publicly accessible article.
                """
            );
    }

    private static async Task<Results<Ok<ArticleResponse>, NotFound>> GetLatestAsync(
        ClaimsPrincipal user,
        ArticleService service,
        CancellationToken cancellation
    )
    {
        var result = await service.GetLatestAsync(user, cancellation);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
