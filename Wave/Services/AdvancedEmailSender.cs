using Microsoft.AspNetCore.Identity.UI.Services;

namespace Wave.Services;

public interface IAdvancedEmailSender : IEmailSender {
	Task SendEmailAsync(string email, string? name, string subject, string htmlMessage);
	Task SendDefaultMailAsync(string receiverMail, string? receiverName, string subject, string title, string bodyHtml);
}