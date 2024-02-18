namespace Wave.Services;

public interface IScopedProcessingService {
	ValueTask DoWork(CancellationToken cancellationToken);
}