namespace Wave.Services;

public interface IEmailService : IAsyncDisposable {
	ValueTask Connect(CancellationToken cancellation);
	ValueTask Disconnect(CancellationToken cancellation);

	ValueTask SendEmailAsync(IEmail email);
}