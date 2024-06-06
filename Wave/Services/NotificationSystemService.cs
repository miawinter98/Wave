using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Wave.Data;
using Wave.Utilities;

namespace Wave.Services;

public enum NotificationImportance {
	Information, Warning, Error, Success
}

public enum NotificationCategory {
	Uncategorized, 
	/* System events like something being saved successfully,
	 * may not be shown in notification panel */
	System, 
	/* Updates on Articles related to you
	 * (it being published, rejected, etc.) */
	ArticleStatus, 
	/* Role related notifications (user submitted for review) */
	Role
}

public record Notification(
	NotificationCategory Category,
	NotificationImportance Importance,
	string ContentResourceKey,
	DateTimeOffset Timestamp,
	params string[] MessageParameters) {

	public IMessageDisplay.Message AsMessage(IStringLocalizer localizer) {
		var title = localizer[ContentResourceKey + "-Title"];
		return new IMessageDisplay.Message(localizer[ContentResourceKey],
			Importance switch {
				NotificationImportance.Information => "alert-info",
				NotificationImportance.Warning => "alert-warning",
				NotificationImportance.Error => "alert-error",
				NotificationImportance.Success => "alert-success",
				var _ => "alert-info"
			},
			title.ResourceNotFound ? null : title.Value,
			Timestamp);
	}
}

public class NotificationSystemService(IDbContextFactory<ApplicationDbContext> context) {
	private IDbContextFactory<ApplicationDbContext> Context { get; } = context;


}