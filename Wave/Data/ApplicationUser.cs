using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Wave.Data;

public class ApplicationUser : IdentityUser {
    public ProfilePicture? ProfilePicture { get; set; }

    [MaxLength(64), PersonalData]
    public string? FullName { get; set; }

    [MaxLength(512), PersonalData]
    public string AboutTheAuthor { get; set; } = string.Empty;
    [MaxLength(4096), PersonalData]
    public string Biography { get; set; } = string.Empty;
    public string BiographyHtml { get; set; } = string.Empty;

    public string Name => FullName ?? UserName ?? "Anon";

    public IList<Article> Articles { get; set; } = [];
    public IList<UserLink> Links { get; set; } = [];
}