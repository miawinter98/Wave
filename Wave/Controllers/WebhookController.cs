using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wave.Data;
using Wave.Data.Api.Mailtrap;
using Wave.Utilities.Metrics;

namespace Wave.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class WebhookController(ILogger<WebhookController> logger, ApplicationDbContext context, ApiMetrics metrics) 
		: ControllerBase {
	[HttpPost("mailtrap/{apiKey}")]
	[Authorize("EmailApi", AuthenticationSchemes = "ApiKeyInRoute")]
	public async Task<IActionResult> Mailtrap(Webhook webhook, string apiKey) {
		logger.LogDebug("Start processing webhook events");
		foreach (var webhookEvent in webhook.Events) {
			metrics.WebhookEventReceived("Mailtrap", webhookEvent.Type.ToString());
			var subscriber = await context.Set<EmailSubscriber>().FirstOrDefaultAsync(s => s.Email == webhookEvent.Email);

			logger.LogDebug("Received {WebhookEvent} event for {email}", webhookEvent.Type, webhookEvent.Email);
			if (subscriber is null) {
				logger.LogWarning(
					"Received {WebhookEvent} from Mailtrap " +
					"but failed to find subscriber with E-Mail {email}.", 
					webhookEvent.Type, webhookEvent.Email);
				metrics.WebhookEventError("Mailtrap", webhookEvent.Type.ToString(), "unknown email");
				continue;
			}

			// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
			switch (webhookEvent.Type) {
				case WebhookEventType.Delivery:
					subscriber.LastMailReceived = webhookEvent.EventDateTime;
					break;
				case WebhookEventType.Open:
					subscriber.LastMailOpened = webhookEvent.EventDateTime;
					break;
				case WebhookEventType.SoftBounce:
					subscriber.UnsubscribeReason = webhookEvent.Response ?? webhookEvent.Type.Humanize(LetterCasing.Title);
					break;
				case WebhookEventType.Suspension:
					logger.LogWarning(
						"Received Suspension event, you may have send from an unverified domain or exceeded your hourly rate.");
					continue;
				case WebhookEventType.Unsubscribe:
					subscriber.Unsubscribed = true;
					subscriber.UnsubscribeReason = "User Unsubscribed";
					break;
				case WebhookEventType.SpamComplaint:
					subscriber.Unsubscribed = true;
					subscriber.UnsubscribeReason = "User reported as Spam";
					break;
				case WebhookEventType.Bounce:
				case WebhookEventType.Reject:
					subscriber.Unsubscribed = true;
					subscriber.UnsubscribeReason = webhookEvent.Reason ?? webhookEvent.Type.Humanize(LetterCasing.Title);
					break;
				case WebhookEventType.Click:
				default:
					logger.LogInformation("Received unsupported event {EventType} for {email}. Skipping.", webhookEvent.Type, webhookEvent.Email);
					metrics.WebhookEventError("Mailtrap", webhookEvent.Type.ToString(), "unknown type");
					continue;
			}

			logger.LogDebug("Webhook event {EventType} for {email} processed successfully.",
				webhookEvent.Type, webhookEvent.Email);
		}
		await context.SaveChangesAsync();
		logger.LogDebug("All webhook events processed and saved");


		return Ok();
	}
}