using Microsoft.Extensions.Options;
using Wave.Data;

namespace Wave.Services;

public class EmailBackgroundWorker(ILogger<EmailBackgroundWorker> logger, IOptions<Features> features, EmailTemplateService templateService, IServiceProvider serviceProvider) : BackgroundService {
	private ILogger<EmailBackgroundWorker> Logger { get; } = logger;
	private Features Features { get; } = features.Value;
	private EmailTemplateService TemplateService { get; } = templateService;
	private IServiceProvider ServiceProvider { get; } = serviceProvider;
	
	protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
		if (!Features.EmailSubscriptions) return;
		
		TemplateService.TryCreateDefaultTemplates();

		Logger.LogInformation("Background email worker starting.");
		
		try {
			// we want this timer to execute every 15 minutes, at fixed times (:00, :15, :30, :45)
			var now = DateTimeOffset.UtcNow;
			int nowMinute = now.Minute;
			int waitTime = 15 - nowMinute % 15;
			// we always want to start a little bit later than :00:00, to make sure we actually distribute the :00:00 newsletters
			if (now.Second < 3) await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
			Logger.LogInformation("First distribution check will be in {waitTime} minutes, at {time}.", 
				waitTime, now.AddMinutes(waitTime).LocalDateTime.ToString("u"));
			await Task.Delay(TimeSpan.FromMinutes(waitTime), stoppingToken);

			using PeriodicTimer timer = new(TimeSpan.FromMinutes(15));
			do {
				await using var scope = ServiceProvider.CreateAsyncScope();
				var service = scope.ServiceProvider.GetRequiredService<NewsletterBackgroundService>();
				await service.DoWork(stoppingToken);
			} while (await timer.WaitForNextTickAsync(stoppingToken));
		} catch (OperationCanceledException) {
			Logger.LogInformation("Background email worker stopping.");
		}
	}
}