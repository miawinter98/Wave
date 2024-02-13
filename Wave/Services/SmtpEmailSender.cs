using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;
using Wave.Data;
using Uri = System.Uri;

namespace Wave.Services;

public class SmtpEmailSender(ILogger<SmtpEmailSender> logger, IOptions<SmtpConfiguration> config, IOptions<Customization> customizations, EmailTemplateService templateService) : IEmailSender<ApplicationUser>, IAdvancedEmailSender {
	private ILogger<SmtpEmailSender> Logger { get; } = logger;
    private SmtpConfiguration Configuration { get; } = config.Value;
	private Customization Customizations { get; } = customizations.Value;
    private EmailTemplateService TemplateService { get; } = templateService;

	public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendEmailAsync(email, "Confirm your email",
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendEmailAsync(email, "Reset your password",
            $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendEmailAsync(email, "Reset your password",
            $"Please reset your password using the following code: {resetCode}");


    public Task SendEmailAsync(string email, string subject, string htmlMessage) {
        return SendEmailAsync(email, null, subject, htmlMessage);
    }

    public async Task SendEmailAsync(string email, string? name, string subject, string htmlMessage) {
        try {
            var message = new MimeMessage {
                From = {new MailboxAddress(Configuration.SenderName, Configuration.SenderEmail)},
                To = { new MailboxAddress(name, email) },
                Subject = subject
            };

            var builder = new BodyBuilder {
                HtmlBody = htmlMessage
            };

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(Configuration.Host, Configuration.Port, 
				Configuration.Ssl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None);
            if (!string.IsNullOrWhiteSpace(Configuration.Username)) {
				await client.AuthenticateAsync(Configuration.Username, Configuration.Password);
			}

			try {
				await client.SendAsync(message);
			} catch (Exception ex) {
				throw new EmailNotSendException("Failed Email send.", ex);
			}
            await client.DisconnectAsync(true);
            Logger.LogInformation("Successfully send mail to {email} (subject: {subject}).", email, subject);
        } catch (Exception ex) {
            Logger.LogError(ex, "Error sending E-Mail");
            throw;
        }
    }

	public Task SendDefaultMailAsync(string receiverMail, string? receiverName, string subject, string title, string bodyHtml) {
		var host = new Uri(string.IsNullOrWhiteSpace(Customizations.AppUrl) ? "" : Customizations.AppUrl);
		string logo = !string.IsNullOrWhiteSpace(Customizations.LogoLink)
			? Customizations.LogoLink
			: new Uri(host, "/img/logo.png").AbsoluteUri;
		string body = TemplateService.Default(host.AbsoluteUri, logo, title, bodyHtml);
		return SendEmailAsync(receiverMail, receiverName, subject, body);
	}
}

public class EmailNotSendException(string message, Exception exception) : ApplicationException(message, exception);