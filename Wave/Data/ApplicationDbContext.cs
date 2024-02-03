using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Wave.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
	: IdentityDbContext<ApplicationUser>(options) {

	protected override void OnModelCreating(ModelBuilder builder) {
		base.OnModelCreating(builder);

		var dateTimeOffsetUtcConverter = new ValueConverter<DateTimeOffset, DateTimeOffset>(
			model => model.ToUniversalTime(),
			utc => utc.ToLocalTime()
			);

		builder.Entity<ApplicationUser>(user => {
			user.Property(u => u.FullName).HasMaxLength(64);
			user.Property(u => u.AboutTheAuthor).HasMaxLength(512);
			user.Property(u => u.Biography).HasMaxLength(4096);

			user.HasOne(u => u.ProfilePicture).WithOne().HasForeignKey(typeof(ProfilePicture))
				.OnDelete(DeleteBehavior.SetNull);
			user.HasMany(u => u.Links).WithOne().OnDelete(DeleteBehavior.Cascade);

			user.Navigation(u => u.ProfilePicture).IsRequired(false);
			user.Navigation(u => u.Links).AutoInclude();
		});
		builder.Entity<ProfilePicture>(pfp => {
			pfp.HasKey(p => p.Id);
			pfp.Property(p => p.ImageId).IsRequired();
			pfp.ToTable("ProfilePictures");
		});

		builder.Entity<Article>(article => {
			article.HasKey(a => a.Id);
			article.Property(a => a.Title)
				.IsRequired().HasMaxLength(256);

			article.HasOne(a => a.Author).WithMany(a => a.Articles)
				.IsRequired().OnDelete(DeleteBehavior.Cascade);
			article.HasOne(a => a.Reviewer).WithMany()
				.IsRequired(false).OnDelete(DeleteBehavior.SetNull);

			article.Property(a => a.CreationDate)
				.IsRequired().HasDefaultValueSql("now()")
				.HasConversion(dateTimeOffsetUtcConverter);
			article.Property(a => a.PublishDate)
				.HasConversion(dateTimeOffsetUtcConverter);
			article.Property(a => a.LastModified)
				.IsRequired().HasDefaultValueSql("now()")
				.HasConversion(dateTimeOffsetUtcConverter);

			article.HasQueryFilter(a => !a.IsDeleted);
			article.ToTable("Articles");
		});

		builder.Entity<Category>(category => {
			category.HasKey(c => c.Id);
			category.Property(c => c.Name).IsRequired().HasMaxLength(128);
			category.Property(c => c.Color).IsRequired().HasSentinel(CategoryColors.Default)
				.HasDefaultValue(CategoryColors.Default);

			category.HasMany(c => c.Articles).WithMany(a => a.Categories)
				.UsingEntity<ArticleCategory>(
					ac => ac.HasOne(a => a.Article).WithMany().OnDelete(DeleteBehavior.NoAction), 
					ac => ac.HasOne(a => a.Category).WithMany().OnDelete(DeleteBehavior.NoAction), 
					articleCategory => {
						articleCategory.HasKey(ac => ac.Id);
						articleCategory.ToTable("ArticleCategories");
						articleCategory.HasQueryFilter(ac => !ac.Article.IsDeleted);
					});
			
			category.ToTable("Categories");
		});
	}
}