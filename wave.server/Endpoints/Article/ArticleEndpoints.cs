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
        article
            .MapGet("{id:guid}", GetArticleAsync)
            .WithName("GetArticleById")
            .WithSummary("Get Article By ID")
            .WithDescription(
                """
                Returns the article with the given ID. This endpoint may also 
                return articles that have not been published if and only if 
                the user has access to it (by either being the author, an assigned
                reviewer or having high access privileges like administrators).
                """
            );
    }

    private static async Task<Results<Ok<ArticleResponse>, NoContent>> GetLatestAsync(
        ClaimsPrincipal user,
        ArticleService service,
        CancellationToken cancellation
    )
    {
        var result = await service.GetLatestAsync(user, cancellation);

        return result is null ? TypedResults.NoContent() : TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ArticleResponse>, NotFound>> GetArticleAsync(
        ClaimsPrincipal user,
        Guid id,
        ArticleService service,
        CancellationToken cancellation
    )
    {
        var result = await service.GetByIdAsync(user, id, cancellation);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
