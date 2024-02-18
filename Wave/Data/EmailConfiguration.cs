namespace Wave.Data;

public class EmailConfiguration {
	public required string SenderEmail { get; init; }
	public required string SenderName { get; init; }
	public required string? ServiceEmail { get; init; }

	public Dictionary<string, SmtpConfiguration> Smtp { get; init; } = new(StringComparer.InvariantCultureIgnoreCase);
}