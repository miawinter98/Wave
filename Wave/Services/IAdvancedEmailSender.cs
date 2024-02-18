using Microsoft.AspNetCore.Identity.UI.Services;
using Wave.Data;

namespace Wave.Services;

[Obsolete]
public interface IAdvancedEmailSender : IEmailSender {
	Task SendEmailAsync(string email, string? name, string subject, string htmlMessage);
	Task SendDefaultMailAsync(string receiverMail, string? receiverName, string subject, string title, string bodyHtml);
	Task SendSubscribedMailAsync(EmailSubscriber subscriber, string subject, string title, string bodyHtml, 
		string browserUrl = "", string subscribedRole = "-1");
	Task SendWelcomeMailAsync(EmailSubscriber subscriber, string subject, string title, string bodyHtml,
		IEnumerable<EmailNewsletter> articles);
}