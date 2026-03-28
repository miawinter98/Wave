using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using wave.server.Configuration;
using wave.server.Data;
using wave.server.Modules.ActivityPub.Models;

namespace wave.server.Modules.ActivityPub.Actor;

public class ActorService(
	IOptions<ActivityPubOptions> options,
	UserManager<ApplicationUser> userManager
)
{
	public async ValueTask<PersonDto?> GetActorAsync(
		string actor,
		string scheme,
		CancellationToken cancellation
	)
	{
#if !DEBUG
		scheme = "https";
#endif
		var user = await userManager.FindByIdAsync(actor);
		if (!actor.Contains('@'))
		{
			user ??= await userManager.Users.FirstOrDefaultAsync(
				u => u.UserName != null && u.UserName == actor,
				cancellation
			);
		}

		if (user is null)
			return null;

		string? username = null;
		if (user.UserName?.Contains('@') is not true)
		{
			username = user.UserName;
		}

		IList<AttachmentDto>? attachments = [];
		if (!string.IsNullOrWhiteSpace(user.AboutTheAuthor))
		{
			attachments.Add(new AttachmentDto("About", user.AboutTheAuthor));
		}
		if (!string.IsNullOrWhiteSpace(user.ContactWebsite))
		{
			attachments.Add(
				new AttachmentDto(
					"Website",
					$"<a href=\"{user.ContactWebsite}\" rel=\"me nofollow noopener\" target=\"_blank\">{user.ContactWebsite}</a>"
				)
			);
		}
		if (attachments.Count < 1)
			attachments = null;

		var localDomain = new Uri(scheme + "://" + options.Value.Domain);
		return new PersonDto(
			new Uri(localDomain, "/activityPub/" + user.Id),
			user.FullName,
			!string.IsNullOrWhiteSpace(user.BiographyHtml) ? $"<p>{user.BiographyHtml}</p>" : null,
			DateTimeOffset.FromUnixTimeMilliseconds(960372000000L),
			new Uri(localDomain, "/activityPub/" + user.Id + "/inbox"),
			new Uri(localDomain, "/activityPub/" + user.Id + "/outbox"),
			null,
			null,
			new Uri(localDomain, "/profile/" + user.Id),
			username,
			false,
			false,
			false,
			null,
			new ImageDto(new Uri(localDomain, $"/api/user/pfp/{user.Id}?size=200")),
			new PublicKeyRecord(
				new Uri(localDomain, "/activityPub/" + user.Id + "#main-key"),
				new Uri(localDomain, "/activityPub/" + user.Id),
				await GetPublicKeyAsync(user.Id)
			),
			attachments?.ToArray()
		);
	}

	public async ValueTask<ApplicationDto> GetMainActorAsync(
		string scheme,
		CancellationToken cancellation
	)
	{
		var localDomain = new Uri(scheme + "://" + options.Value.Domain);

		return new ApplicationDto(
			new Uri(localDomain, "/activityPub/actor"),
			new Uri(localDomain, "/activityPub/actor/inbox"),
			new Uri(localDomain, "/activityPub/actor/outbox"),
			null,
			null,
			"actor",
			true,
			false,
			new PublicKeyRecord(
				new Uri(localDomain, "/activityPub/actor#main-key"),
				new Uri(localDomain, "/activityPub/actor"),
				await GetPublicKeyAsync("actor")
			)
		);
	}

	public async ValueTask<OrderedCollectionResponse<object>?> GetOutboxAsync(
		string actor,
		string scheme,
		CancellationToken cancellation
	)
	{
		var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == actor, cancellation);
		if (user is null)
			return null;

		// TODO implement with articles

		return new OrderedCollectionResponse<object>(null, 0, null, null);
	}

	private async ValueTask<string> GetPublicKeyAsync(string actor)
	{
		var keyDirectory = Path.Combine("configuration", "keys");
		var file = Path.Combine(keyDirectory, actor + ".pem");

		if (File.Exists(file))
		{
			var pub = await File.ReadAllTextAsync(file);
			using var rsa = RSA.Create();
			rsa.ImportFromPem(pub);
			return rsa.ExportSubjectPublicKeyInfoPem();
		}

		if (Directory.Exists(keyDirectory) is not true)
		{
			Directory.CreateDirectory(keyDirectory);
		}

		string publicKey;
		string privateKey;
		using (var rsa = RSA.Create())
		{
			rsa.KeySize = 4096;
			publicKey = rsa.ExportRSAPublicKeyPem();
			privateKey = rsa.ExportRSAPrivateKeyPem();
		}

		await File.WriteAllTextAsync(Path.Combine(keyDirectory, actor + ".pem"), publicKey);
		await File.WriteAllTextAsync(
			Path.Combine(keyDirectory, actor + ".private.pem"),
			privateKey
		);

		return publicKey;
	}
}
