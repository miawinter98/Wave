using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using wave.server.Configuration;
using wave.server.Data;

namespace wave.server.Modules.ActivityPub.WebFinger;

public class WebFingerService(
	IOptions<ActivityPubOptions> options,
	UserManager<ApplicationUser> userService
)
{
	public async ValueTask<ActorResponse?> GetActorAsync(
		string resourceName,
		string scheme,
		CancellationToken cancellation
	)
	{
		var localDomain = scheme + "://" + options.Value.Domain;

		var data = resourceName.Split("@", 2);
		if (data.Length < 2)
			return null;
		var accountName = data[0];
		var accountDomain = data[1];

		if (
			!Uri.TryCreate(scheme + "://" + accountDomain, UriKind.Absolute, out var d)
			|| !new Uri(localDomain, UriKind.Absolute).Equals(d)
		)
		{
			return null;
		}

		var user = await userService.FindByIdAsync(accountName);

		if (accountName.Contains('@') is not true)
			user ??= await userService.Users.FirstOrDefaultAsync(
				u => u.UserName != null && u.UserName == accountName,
				cancellation
			);

		if (user is null)
			return null;

		IList<string> aliases =
		[
			new Uri(new Uri(localDomain, UriKind.Absolute), "/activityPub/" + user.Id).ToString(),
		];

		if (user.UserName is { } name && !name.Contains('@'))
		{
			aliases.Add(
				new Uri(new Uri(localDomain, UriKind.Absolute), "/activityPub/" + name).ToString()
			);
		}

		return new ActorResponse(
			"acct:" + resourceName,
			aliases,
			[
				new LinkDto(
					"self",
					"application/activity+json",
					new Uri(new Uri(localDomain, UriKind.Absolute), $"/activityPub/{user.Id}"),
					null
				),
				new LinkDto(
					"http://webfinger.net/rel/profile-page",
					"text/html",
					new Uri(new Uri(localDomain, UriKind.Absolute), "/profile/" + user.Id),
					null
				),
				new LinkDto(
					"http://webfinger.net/rel/avatar",
					"image/webp",
					new Uri(
						new Uri(localDomain, UriKind.Absolute),
						"/api/user/pfp/" + user.Id + "?size=400"
					),
					null
				),
			]
		);
	}
}
