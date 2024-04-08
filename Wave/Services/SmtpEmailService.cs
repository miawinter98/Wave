using MailKit;
using MailKit.Net.Smtp;
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

	public async ValueTask SendEmailAsync(IEmail email, CancellationToken cancellation = default) {
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
				await Client.SendAsync(message, cancellation);
				Logger.LogInformation("Successfully send mail to {email} (subject: {subject}).",
					email.ReceiverEmail, email.Subject);
				return;
			} catch (ServiceNotConnectedException ex) {
				Logger.LogWarning(ex, "Not connected, attempting reconnect.");
				if (!await TryReconnect(cancellation)) throw;
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

	private async ValueTask<bool> TryReconnect(CancellationToken cancellation) {
		int reconnectCount = 0;
		while (true) {
			try {
				await DisconnectAsync(cancellation);
				await ConnectAsync(cancellation);
				return true;
			} catch (Exception ex1) {
				reconnectCount++;
				// 2^11 = 2048 seconds ~= 34 minutes
				if (reconnectCount <= 11) {
					// 2 4 6 8 16 32 64... seconds
					int waitTime = (int)Math.Pow(2, reconnectCount);
					Logger.LogError(ex1, "Reconnect failed, Try: {ReconnectTry}. Will wait {ReconnectWaitTime} Seconds for next attempt.", reconnectCount, waitTime);
					await Task.Delay(TimeSpan.FromSeconds(waitTime), cancellation);
				} else {
					Logger.LogCritical(ex1, "Reconnect retry count exceeded, giving up.");
					return false;
				}
			}
		}
	}
}