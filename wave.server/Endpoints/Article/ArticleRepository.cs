using Microsoft.EntityFrameworkCore;
using wave.server.Data;

namespace wave.server.Endpoints.Article;

public class ArticleRepository(IDbContextFactory<DatabaseContext> context)
{
    public async ValueTask<ArticleRecord?> GetLatestAsync(CancellationToken cancellation)
    {
        await using var dao = await context.CreateDbContextAsync(cancellation);

        var now = DateTimeOffset.UtcNow;
        return await dao.Set<ArticleRecord>()
            .Where(a => a.Status >= ArticleStatus.Published && a.PublishDate < now)
            .FirstOrDefaultAsync(cancellation);
    }
}
