using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace wave.server.Data;

public class DatabaseContext : IdentityDbContext<ApplicationUser>
{
	public DatabaseContext(DbContextOptions options)
		: base(options) { }

	protected DatabaseContext() { }

	protected override void OnModelCreating(ModelBuilder model)
	{
		model.Entity<ApplicationUser>(user =>
		{
			user.Property(u => u.FullName).HasMaxLength(64);
			user.Property(u => u.AboutTheAuthor).HasMaxLength(512);
			user.Property(u => u.ContactWebsite).HasMaxLength(128);
			user.Property(u => u.Biography).HasMaxLength(4096);
			user.Property(u => u.BiographyHtml).HasMaxLength(-1);
		});

		// Introduced in .net 10, can be added some other time when we not
		// transitioning the backend
		model.Ignore<IdentityPasskeyData>();

		model.Entity<ArticleRecord>(article =>
		{
			article.ToTable("Articles");

			article.HasKey(a => a.Id);
			article.Property(a => a.Title).HasMaxLength(256);
			article.Property(a => a.Body).IsRequired();
			article.Property(a => a.Slug).HasMaxLength(64);
			article.Property(a => a.CreationDate).HasDefaultValueSql("now()");

			article
				.HasOne(a => a.Author)
				.WithMany(a => a.Articles)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);
		});

		base.OnModelCreating(model);
	}
}
