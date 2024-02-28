using Microsoft.AspNetCore.Identity;
using Wave.Data;

namespace Wave.Services;

public class SmtpEmailSender(EmailFactory email, [FromKeyedServices("live")]IEmailService emailService) : IEmailSender<ApplicationUser>, IAsyncDisposable {
	private EmailFactory Email { get; } = email;
	private IEmailService EmailService { get; } = emailService;

	#region IEmailSenderAsync<ApplicationUser>
	
	public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
		SendDefaultMailAsync(email, user.FullName, "Confirm your email", "Confirm your email",
			$"<p>Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.</p>",
			$"Please confirm your account by clicking here: {confirmationLink}");

	public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
		SendDefaultMailAsync(email, user.FullName, "Reset your password", "Reset your password",
			$"<p>Please reset your password by <a href='{resetLink}'>clicking here</a>.</p>",
			$"Please reset your password by clicking here: {resetLink}");

	public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
		SendDefaultMailAsync(email, user.FullName, "Reset your password", "Reset your password",
			$"<p>Please reset your password using the following code: {resetCode}.</p>",
			$"Please reset your password using the following code: {resetCode}.");

	#endregion
	
	private async Task SendDefaultMailAsync(string receiverMail, string? receiverName, string subject, string title, string bodyHtml, string bodyPlain) {
		await EmailService.ConnectAsync(CancellationToken.None);
		var email = await Email.CreateDefaultEmail(receiverMail, receiverName, subject, title, bodyHtml, bodyPlain);
		await EmailService.SendEmailAsync(email);
		await EmailService.DisconnectAsync(CancellationToken.None);
	}

	public async ValueTask DisposeAsync() {
		GC.SuppressFinalize(this);
		await EmailService.DisposeAsync();
	}
}