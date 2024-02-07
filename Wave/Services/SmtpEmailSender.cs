using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using Wave.Data;

namespace Wave.Services;

public class SmtpEmailSender(IOptions<SmtpConfiguration> config, ILogger<SmtpEmailSender> logger) : IEmailSender<ApplicationUser>, IEmailSender {
    private SmtpConfiguration Configuration { get; } = config.Value;
    private ILogger<SmtpEmailSender> Logger { get; } = logger;
    
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
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        } catch (Exception ex) {
            Logger.LogError(ex, "Error sending E-Mail");
            throw;
        }
    }
}