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
	// ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
	public string BiographyHtml { get; set; } = string.Empty;

    public string Name => FullName ?? "Guest Author";

    public IList<Article> Articles { get; set; } = [];
    [PersonalData]
    public IList<UserLink> Links { get; set; } = [];

    [MaxLength(128), EmailAddress, PersonalData]
	public string ContactEmail { get; set; } = string.Empty;
	[MaxLength(64), Phone, PersonalData]
	public string ContactPhone { get; set; } = string.Empty;
	[MaxLength(64), Phone, PersonalData]
	public string ContactPhoneBusiness { get; set; } = string.Empty;
	[MaxLength(128), Phone, PersonalData]
	public string ContactWebsite { get; set; } = string.Empty;
}