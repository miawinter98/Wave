namespace Wave.Data.Api;

public record UserDto(
	string Name,
	string ProfilePictureUrl,
	string? ProfileUrl) {

	public static UserDto GetFromUser(ApplicationUser user, Uri host, int pfpSize) {
		var pfpUrl = new Uri(host, $"/api/User/pfp/{user.Id}?size={pfpSize}");
		var profileUrl = user.Articles.Count > 0 ? new Uri(host, $"/profile/{user.Id}") : null;

		return new UserDto(user.FullName ?? "Guest Author", pfpUrl.AbsoluteUri, profileUrl?.AbsoluteUri);
	}
}