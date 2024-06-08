using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Wave.Data.Transactional;

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
			article.OwnsMany(a => a.Headings);

			article.Property(a => a.CreationDate)
				.IsRequired().HasDefaultValueSql("now()")
				.HasConversion(dateTimeOffsetUtcConverter);
			article.Property(a => a.PublishDate)
				.HasConversion(dateTimeOffsetUtcConverter);
			article.Property(a => a.LastModified)
				.IsRequired().HasDefaultValueSql("now()")
				.HasConversion(dateTimeOffsetUtcConverter);
			
			article.Property(a => a.BodyPlain).HasDefaultValue("");

			article.Property(a => a.CanBePublic)
				.HasComputedColumnSql(
					$"\"{nameof(Article.IsDeleted)}\" = false AND \"{nameof(Article.Status)}\" = {(int)ArticleStatus.Published}", true);

			article.HasQueryFilter(a => a.CanBePublic && a.PublishDate <= DateTimeOffset.UtcNow);
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
					ac => ac.HasOne(a => a.Article).WithMany().OnDelete(DeleteBehavior.Cascade), 
					ac => ac.HasOne(a => a.Category).WithMany().OnDelete(DeleteBehavior.Restrict), 
					articleCategory => {
						articleCategory.HasKey(ac => ac.Id);
						articleCategory.ToTable("ArticleCategories");
						articleCategory.HasQueryFilter(ac => 
							ac.Article.CanBePublic && ac.Article.PublishDate <= DateTimeOffset.UtcNow);
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

			newsletter.HasQueryFilter(n => n.Article.CanBePublic && n.Article.PublishDate <= DateTimeOffset.UtcNow);
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

	internal async ValueTask UpdateArticle(ArticleDto dto, Article article, 
			CancellationToken cancellation) {
		article.Title = dto.Title;
		article.Body = dto.Body;
		article.LastModified = DateTimeOffset.UtcNow;
		article.UpdateBody();

		if (article.CanBePublic is false || article.PublishDate > DateTimeOffset.UtcNow) {
			// Update publish date, if it exists and article isn't public yet
			if (dto.PublishDate is {} date) article.PublishDate = date;
			// Can only change slugs when the article is not public
			article.UpdateSlug(dto.Slug);
		}

		await UpdateCategories(dto, article, cancellation);
		await UpdateImages(dto, article, cancellation);
		await UpdateNewsletter(article, cancellation);
	}
	
	private async ValueTask UpdateCategories(ArticleDto dto, Article article, CancellationToken cancellation) {
		if (dto.Categories is null) return;
		
		// Retrieve all existing links between this article and categories
		var relationships = await Set<ArticleCategory>()
			.IgnoreAutoIncludes()
			.Include(ac => ac.Category)
			.Where(ac => ac.Article == article)
			.ToListAsync(cancellation);
		
		// check which Category is not in the DTO and needs its relationship removed
		var removed = relationships.Where(ac => !dto.Categories.Contains(ac.Category.Id)).ToList();
		if(removed.Count > 0) RemoveRange(removed);
		
		// check which Category in the DTO is absent from the article's relationships, and add them
		var added = dto.Categories.Where(cId => relationships.All(ac => ac.Category.Id != cId)).ToList();
		if (added.Count > 0) {
			var categories = await Set<Category>()
				.IgnoreAutoIncludes().IgnoreQueryFilters()
				.Where(c => added.Contains(c.Id))
				.ToListAsync(cancellation);

			await AddRangeAsync(categories.Select(c => new ArticleCategory {
				Article = article, Category = c
			}).ToList(), cancellation);
		}
	}

	private async ValueTask UpdateImages(ArticleDto dto, Article article, CancellationToken cancellation) {
		if (dto.Images is null) return;

		// TODO:: implement
	}

	private async ValueTask UpdateNewsletter(Article article, CancellationToken cancellation) {
		// Update Newsletter distribution if it exists
		var newsletter = await Set<EmailNewsletter>()
			.IgnoreQueryFilters().IgnoreAutoIncludes()
			.FirstOrDefaultAsync(n => n.Article == article, cancellation);
		if (newsletter is not null) newsletter.DistributionDateTime = article.PublishDate;
	}
}