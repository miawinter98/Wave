namespace Wave.Services;

public interface IEmailService : IAsyncDisposable {
	ValueTask ConnectAsync(CancellationToken cancellation);
	ValueTask DisconnectAsync(CancellationToken cancellation);

	ValueTask SendEmailAsync(IEmail email);
}