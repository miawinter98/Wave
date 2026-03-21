using Microsoft.AspNetCore.Identity;
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
		var accountName = data[0];
		var accountDomain = data[1];

		if (
			!Uri.TryCreate(accountDomain, UriKind.Absolute, out var d)
			|| !new Uri(options.Value.Domain).IsBaseOf(d)
		)
			return null;

		var user = await userService.FindByIdAsync(accountName);
		if (user is null)
			return null;

		return new ActorResponse(
			"acct:" + resourceName,
			[new Uri(new Uri(localDomain, UriKind.Absolute), "/api/user/" + user.Id).ToString()],
			[
				new LinkDto(
					"self",
					"application/json",
					new Uri(new Uri(localDomain, UriKind.Absolute), $"/api/user/{user.Id}"),
					null
				),
			]
		);
	}
}
