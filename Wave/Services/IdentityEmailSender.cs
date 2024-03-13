using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Wave.Data;

namespace Wave.Services;

public class IdentityEmailSender(EmailFactory email, [FromKeyedServices("live")]IEmailService emailService, IStringLocalizer<IdentityEmailSender> localizer) : IEmailSender<ApplicationUser>, IAsyncDisposable {
	private EmailFactory Email { get; } = email;
	private IEmailService EmailService { get; } = emailService;
	private IStringLocalizer<IdentityEmailSender> Localizer { get; } = localizer;

	#region IEmailSenderAsync<ApplicationUser>
	
	public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
		SendDefaultMailAsync(email, user.FullName, Localizer["ConfirmationMail_Subject"], Localizer["ConfirmationMail_Title"],
			$"<p>{Localizer["ConfirmationMail_Body"]}</p>" +
			$"<p style=\"text-align: center\"><a href=\"{confirmationLink}\">{Localizer["ConfirmName_LinkLabel"]}</a></p>",
			$"{Localizer["ConfirmationMail_Body"]} {confirmationLink}");

	public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
		SendDefaultMailAsync(email, user.FullName, Localizer["PasswordResetMail_Subject"], Localizer["PasswordResetMail_Title"],
			$"<p>{Localizer["PasswordResetMail_Body"]}</p>" +
			$"<p style=\"text-align: center\"><a href=\"{resetLink}\">{Localizer["PasswordResetMail_LinkLabel"]}</a>.</p>",
			$"{Localizer["PasswordResetMail_Body"]} {resetLink}");

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