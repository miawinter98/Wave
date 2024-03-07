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
			article.Property(a => a.Slug).HasMaxLength(64).IsRequired().HasDefaultValue("");

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
			
			article.Property(a => a.BodyPlain).HasDefaultValue("");

			article.HasQueryFilter(a => !a.IsDeleted && a.Status >= ArticleStatus.Published && a.PublishDate <= DateTimeOffset.UtcNow);
			article.ToTable("Articles");
		});

		builder.HasCollation("default-case-insensitive", "und-u-kf-upper-ks-level1", "icu", false);
		builder.Entity<Category>(category => {
			category.HasKey(c => c.Id);
			category.Property(c => c.Name).IsRequired().HasMaxLength(128).UseCollation("default-case-insensitive");
			category.HasIndex(c => c.Name).IsUnique();
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
		builder.Entity<ArticleImage>(img => {
			img.HasKey(i => i.Id);
			img.Property(i => i.ImageDescription).IsRequired().HasMaxLength(2048);

			img.HasOne<Article>().WithMany(a => a.Images).OnDelete(DeleteBehavior.Cascade);

			img.ToTable("Images");
		});

		builder.Entity<EmailNewsletter>(newsletter => {
			newsletter.HasKey(n => n.Id);
			newsletter.HasOne(n => n.Article).WithOne().HasForeignKey<EmailNewsletter>()
				.IsRequired().OnDelete(DeleteBehavior.Cascade);
			newsletter.Property(a => a.DistributionDateTime)
				.HasConversion(dateTimeOffsetUtcConverter);

			newsletter.ToTable("Newsletter");
		});
		builder.Entity<EmailSubscriber>(subscriber => {
			subscriber.HasKey(s => s.Id);

			subscriber.Property(s => s.Name).IsRequired(false).HasMaxLength(128);
			subscriber.Property(s => s.Email).IsRequired().HasMaxLength(256).UseCollation("default-case-insensitive");
			subscriber.HasIndex(s => s.Email).IsUnique();
			subscriber.Property(s => s.Language).IsRequired().HasMaxLength(8).HasDefaultValue("en-US");

			subscriber.Property(s => s.UnsubscribeReason).HasMaxLength(256);
			subscriber.Property(s => s.LastMailReceived).HasConversion(dateTimeOffsetUtcConverter);
			subscriber.Property(s => s.LastMailOpened).HasConversion(dateTimeOffsetUtcConverter);

			subscriber.HasIndex(s => s.Unsubscribed);

			subscriber.HasQueryFilter(s => !s.Unsubscribed);
			subscriber.ToTable("NewsletterSubscribers");
		});

		builder.Entity<ApiKey>(key => {
			key.HasKey(k => k.Key);
			key.Property(k => k.Key).IsRequired().HasMaxLength(128);
			key.Property(k => k.OwnerName).IsRequired().HasMaxLength(128);

			key.HasMany(k => k.ApiClaims).WithOne().OnDelete(DeleteBehavior.Cascade);
			key.Ignore(k => k.Claims);
		});
	}
}