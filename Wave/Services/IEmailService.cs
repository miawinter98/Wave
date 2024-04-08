namespace Wave.Services;

public interface IEmailService : IAsyncDisposable {
	ValueTask ConnectAsync(CancellationToken cancellation);
	ValueTask DisconnectAsync(CancellationToken cancellation);

	ValueTask SendEmailAsync(IEmail email, CancellationToken cancellation = default);
}

public sealed class NoOpEmailService : IEmailService {
	public ValueTask DisposeAsync() => ValueTask.CompletedTask;

	public ValueTask ConnectAsync(CancellationToken cancellation) => ValueTask.CompletedTask;
	public ValueTask DisconnectAsync(CancellationToken cancellation) => ValueTask.CompletedTask;
	public ValueTask SendEmailAsync(IEmail email, CancellationToken cancellation = default) => ValueTask.CompletedTask;
}