using Microsoft.AspNetCore.Identity;

namespace Wave.Data;

public class ApplicationUser : IdentityUser {
    public ProfilePicture? ProfilePicture { get; set; }

    public string Name => UserName ?? Email ?? Id;
}