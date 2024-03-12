using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wave.Data;
using Wave.Data.Api.Mailtrap;

namespace Wave.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class WebhookController(ILogger<WebhookController> logger, ApplicationDbContext context) : ControllerBase {
	[HttpPost("mailtrap/{apiKey}")]
	[Authorize("EmailApi", AuthenticationSchemes = "ApiKeyInRoute")]
	public async Task<IActionResult> Mailtrap(Webhook webhook, string apiKey) {
		Console.WriteLine(apiKey);
		foreach (var webhookEvent in webhook.Events) {
			var subscriber = await context.Set<EmailSubscriber>().FirstOrDefaultAsync(s => s.Email == webhookEvent.Email);

			if (subscriber is null) {
				logger.LogWarning(
					"Received webhook event from mailtrap of type {type}, but failed to find subscriber with E-Mail {email}.", 
					webhookEvent.Type, webhookEvent.Email);
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
					subscriber.Unsubscribed = true;
					subscriber.UnsubscribeReason ??= "unknown";
					break;
				case WebhookEventType.Unsubscribe:
					subscriber.Unsubscribed = true;
					subscriber.UnsubscribeReason ??= "User Unsubscribed";
					break;
				case WebhookEventType.SpamComplaint:
					subscriber.Unsubscribed = true;
					subscriber.UnsubscribeReason ??= "User reported as Spam";
					break;
				case WebhookEventType.Reject:
					subscriber.Unsubscribed = true;
					subscriber.UnsubscribeReason ??= webhookEvent.Reason?.Humanize().Titleize() ?? "Rejected";
					break;
			}

			await context.SaveChangesAsync();
		}


		return Ok();
	}
}