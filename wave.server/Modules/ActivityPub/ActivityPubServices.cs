using wave.server.Modules.ActivityPub.Actor;
using wave.server.Modules.ActivityPub.WebFinger;

namespace wave.server.Modules.ActivityPub;

public static class ActivityPubServices
{
	public static IServiceCollection AddActivityPub(this IServiceCollection services)
	{
		services.AddScoped<WebFingerService>();
		services.AddScoped<ActorService>();

		return services;
	}
}
