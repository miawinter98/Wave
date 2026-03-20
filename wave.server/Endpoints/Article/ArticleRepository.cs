using Microsoft.EntityFrameworkCore;
using wave.server.Data;

namespace wave.server.Endpoints.Article;

public class ArticleRepository(IDbContextFactory<DatabaseContext> context)
{
    public async ValueTask<ArticleRecord?> GetLatestAsync(CancellationToken cancellation)
    {
        await using var dao = await context.CreateDbContextAsync(cancellation);

        return await dao.Set<ArticleRecord>().FirstOrDefaultAsync(cancellation);
    }
}
