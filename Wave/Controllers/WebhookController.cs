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
		foreach (var webhookEvent in webhook.Events) {
			metrics.WebhookEventReceived("Mailtrap", webhookEvent.Type.ToString());
			var subscriber = await context.Set<EmailSubscriber>().FirstOrDefaultAsync(s => s.Email == webhookEvent.Email);

			logger.LogDebug("Received Webhook event {EventType} for {email}", 
				webhookEvent.Type, webhookEvent.Email);

			if (subscriber is null) {
				logger.LogWarning(
					"Received webhook event from mailtrap of type {EventType}, " +
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
				case WebhookEventType.Bounce:
					// Store this message in case it develops into a suspension
					subscriber.UnsubscribeReason = webhookEvent.Response;
					break;
				case WebhookEventType.Suspension:
					logger.LogWarning("Received Suspension event, you may have send from an unverifyied domain or exceeded your hourly rate.");
					return Ok();
					break;
				case WebhookEventType.Unsubscribe:
					subscriber.Unsubscribed = true;
					subscriber.UnsubscribeReason = "User Unsubscribed";
					break;
				case WebhookEventType.SpamComplaint:
					subscriber.Unsubscribed = true;
					subscriber.UnsubscribeReason = "User reported as Spam";
					break;
				case WebhookEventType.Reject:
					subscriber.Unsubscribed = true;
					subscriber.UnsubscribeReason = webhookEvent.Reason?.Humanize().Titleize() ?? "Rejected";
					break;
				case WebhookEventType.SoftBounce:
				case WebhookEventType.Click:
				default:
					logger.LogInformation("Received unsupported event {EventType} for {email}. Skipping.", webhookEvent.Type, webhookEvent.Email);
					metrics.WebhookEventError("Mailtrap", webhookEvent.Type.ToString(), "unknown type");
					return Ok();
			}

			await context.SaveChangesAsync();
			logger.LogDebug("Webhook event {EventType} for {email} processed successfully.",
				webhookEvent.Type, webhookEvent.Email);
		}


		return Ok();
	}
}