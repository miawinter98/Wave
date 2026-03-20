using System.Security.Claims;

namespace wave.server.Endpoints.Article;

public class ArticleService(ArticleRepository repository)
{
    public async ValueTask<ArticleResponse?> GetLatestAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellation
    )
    {
        var result = await repository.GetLatestAsync(cancellation);
        return result is null ? null : new ArticleResponse(result);
    }
}
