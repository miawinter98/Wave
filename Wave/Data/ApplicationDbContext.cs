using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Wave.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options) {

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(user => {
            user.HasOne(u => u.ProfilePicture).WithOne().HasForeignKey(typeof(ProfilePicture))
                .OnDelete(DeleteBehavior.SetNull);

            user.Navigation(u => u.ProfilePicture).IsRequired(false);
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

            article.HasOne(a => a.Author).WithMany()
                .IsRequired().OnDelete(DeleteBehavior.Cascade);
            article.HasOne(a => a.Reviewer).WithMany()
                .IsRequired(false).OnDelete(DeleteBehavior.SetNull);

            article.Property(a => a.CreationDate)
                .IsRequired().HasDefaultValueSql("now()");
            article.Property(a => a.LastModified)
                .IsRequired().HasDefaultValueSql("now()");

            article.HasQueryFilter(a => !a.IsDeleted);
        });
    }
}