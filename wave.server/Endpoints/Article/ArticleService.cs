using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using wave.server.Data;

namespace wave.server.Endpoints.Article;

public class ArticleService(ArticleRepository repository, UserManager<ApplicationUser> userService)
{
    public async ValueTask<ArticleResponse?> GetLatestAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellation
    )
    {
        var result = await repository.GetLatestAsync(cancellation);
        return result is null ? null : new ArticleResponse(result);
    }

    internal async ValueTask<ArticleResponse?> GetByIdAsync(
        ClaimsPrincipal principal,
        Guid id,
        CancellationToken cancellation
    )
    {
        var article = await repository.GetByIdAsync(id, cancellation);

        if (article is null)
            return null;

        // Article is public
        if (
            article.Status >= ArticleStatus.Published
            && article.PublishDate <= DateTimeOffset.UtcNow
        )
            return new ArticleResponse(article);

        var user = await userService.GetUserAsync(principal);
        if (user is null)
            return null;

        // User is Article Author
        if (user.Equals(article.Author))
            return new ArticleResponse(article);

        // TODO check permissions

        throw new NotImplementedException();
    }
}
