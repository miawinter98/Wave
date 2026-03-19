using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using wave.server.Data;

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
        IDbContextFactory<DatabaseContext> context,
        CancellationToken cancellation
    )
    {
        await using var dao = await context.CreateDbContextAsync(cancellation);
        var article = await dao.Set<ArticleRecord>().FirstOrDefaultAsync(cancellation);

        return article is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(new ArticleResponse(article));
    }
}
