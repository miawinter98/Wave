using Microsoft.AspNetCore.Identity;
using Wave.Data;

namespace Wave.Services;

public class SmtpEmailSender(EmailFactory email, [FromKeyedServices("live")]IEmailService emailService, [FromKeyedServices("bulk")]IEmailService bulkEmailService) : IEmailSender<ApplicationUser>, IAdvancedEmailSender, IAsyncDisposable {
	private EmailFactory Email { get; } = email;
	private IEmailService EmailService { get; } = emailService;
	private IEmailService BulkEmailService { get; } = bulkEmailService;

	#region IEmailSenderAsync<ApplicationUser>
	
	public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
		SendEmailAsync(email, user.FullName, "Confirm your email",
			$"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

	public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
		SendEmailAsync(email, user.FullName, "Reset your password",
			$"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

	public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
		SendEmailAsync(email, user.FullName, "Reset your password",
			$"Please reset your password using the following code: {resetCode}");

	#endregion

	#region IEmailSender 
	
	public Task SendEmailAsync(string email, string subject, string htmlMessage) {
		return SendEmailAsync(email, null, subject, htmlMessage);
	}

	#endregion


	public async Task SendEmailAsync(string email, string? name, string subject, string htmlMessage) {
		await EmailService.ConnectAsync(CancellationToken.None);
		await EmailService.SendEmailAsync(await Email.CreateDefaultEmail(email, name, subject, subject, htmlMessage));
		await EmailService.DisconnectAsync(CancellationToken.None);
	}
	
	public async Task SendDefaultMailAsync(string receiverMail, string? receiverName, string subject, string title, string bodyHtml) {
		await EmailService.ConnectAsync(CancellationToken.None);
		var email = await Email.CreateDefaultEmail(receiverMail, receiverName, subject, title, bodyHtml);
		await EmailService.SendEmailAsync(email);
		await EmailService.DisconnectAsync(CancellationToken.None);
	}

	public async Task SendSubscribedMailAsync(EmailSubscriber subscriber, string subject, string title, string bodyHtml, 
			string browserUrl = "", string subscribedRole = "-1") {
		var email = await Email.CreateSubscribedEmail(subscriber, browserUrl, subject, title, bodyHtml, subscribedRole);
		await BulkEmailService.ConnectAsync(CancellationToken.None);
		await BulkEmailService.SendEmailAsync(email);
	}

	public async Task SendWelcomeMailAsync(EmailSubscriber subscriber, string subject, string title, string bodyHtml, 
		IEnumerable<EmailNewsletter> articles) {
		var email = await Email.CreateWelcomeEmail(subscriber, articles, subject, title, bodyHtml);
		await BulkEmailService.ConnectAsync(CancellationToken.None);
		await BulkEmailService.SendEmailAsync(email); 
	}

	public async ValueTask DisposeAsync() {
		GC.SuppressFinalize(this);
		await EmailService.DisposeAsync();
		await BulkEmailService.DisposeAsync();
	}
}