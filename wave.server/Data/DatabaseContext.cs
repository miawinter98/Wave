using Microsoft.EntityFrameworkCore;

namespace wave.server.Data;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions options)
        : base(options) { }

    protected DatabaseContext() { }

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<ArticleRecord>(article =>
        {
            article.ToTable("Articles");

            article.HasKey(a => a.Id);
            article.Property(a => a.Title).HasMaxLength(256);
            article.Property(a => a.Body).IsRequired();
            article.Property(a => a.Slug).HasMaxLength(64);
        });
    }
}
