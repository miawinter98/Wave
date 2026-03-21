using Microsoft.AspNetCore.Identity;

namespace wave.server.Data;

public class ApplicationUser : IdentityUser
{
	[PersonalData]
	public string? FullName { get; set; }

	public IList<ArticleRecord> Articles { get; } = [];
}
