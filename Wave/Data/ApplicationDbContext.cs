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
    }
}