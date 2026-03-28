using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace wave.server.Data;

public class ApplicationUser : IdentityUser
{
	[PersonalData]
	public string? FullName { get; set; }

	[PersonalData]
	public string Biography { get; set; } = string.Empty;

	[PersonalData]
	public string BiographyHtml { get; set; } = string.Empty;

	[PersonalData]
	public string AboutTheAuthor { get; set; } = string.Empty;

	[Url, PersonalData]
	public string ContactWebsite { get; set; } = string.Empty;

	public IList<ArticleRecord> Articles { get; } = [];
}
