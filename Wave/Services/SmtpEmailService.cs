﻿using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Wave.Data;

namespace Wave.Services;

public class SmtpEmailService(ILogger<SmtpEmailService> logger, IOptions<EmailConfiguration> emailConfiguration, SmtpConfiguration configuration) : IEmailService {
	private ILogger<SmtpEmailService> Logger { get; } = logger;
	private EmailConfiguration EmailConfiguration { get; } = emailConfiguration.Value;
	private SmtpConfiguration Configuration { get; } = configuration;

	private SmtpClient? Client { get; set; }

	public async ValueTask DisposeAsync() {
		GC.SuppressFinalize(this);
		await DisconnectAsync(CancellationToken.None);
	}

	public async ValueTask ConnectAsync(CancellationToken cancellation) {
		if (Client is not null) return;

		try {
			Client = new SmtpClient();
			await Client.ConnectAsync(Configuration.Host, Configuration.Port,
				Configuration.Ssl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None, cancellation);

			if (!string.IsNullOrWhiteSpace(Configuration.Username)) {
				await Client.AuthenticateAsync(Configuration.Username, Configuration.Password, cancellation);
			}
		} catch (Exception ex) {
			Logger.LogError(ex, "Error connecting to SMTP Client.");
			Client?.Dispose();
			throw;
		}
	}

	public async ValueTask DisconnectAsync(CancellationToken cancellation) {
		if (Client is null) return;
		await Client.DisconnectAsync(true, cancellation);
		Client.Dispose();
		Client = null;
	}

	public async ValueTask SendEmailAsync(IEmail email) {
		if (Client is null) throw new ApplicationException("Not connected.");
		
		var message = new MimeMessage {
			From = { new MailboxAddress(EmailConfiguration.SenderName, EmailConfiguration.SenderEmail) },
			To = { new MailboxAddress(email.ReceiverName, email.ReceiverEmail) },
			Subject = email.Subject
		};
		var builder = new BodyBuilder {
			HtmlBody = email.ContentHtml,
			TextBody = email.ContentPlain
		};
		message.Body = builder.ToMessageBody();
		foreach ((string id, string value) in email.Headers) {
			if (id == HeaderId.ListUnsubscribe.ToHeaderName()) {
				message.Headers.Add(HeaderId.ListId, $"<mailto:{EmailConfiguration.ServiceEmail ?? EmailConfiguration.SenderEmail}>");
			}
			message.Headers.Add(id, value);
		}

		int retryCount = 0;
		while (retryCount < 3) {
			try {
				await Client.SendAsync(message);
				Logger.LogInformation("Successfully send mail to {email} (subject: {subject}).",
					email.ReceiverEmail, email.Subject);
				return;
			} catch (Exception ex) {
				retryCount++;
				Logger.LogWarning(ex, "Error sending E-Mail to {email}. Try: {RetryCount}.", 
					email.ReceiverEmail, retryCount);
			}
		}
		// TODO enqueue for re-sending or throw exception if applicable and handle hard bounce
		// throw new EmailNotSendException();
		Logger.LogError("Giving up");
	}
}