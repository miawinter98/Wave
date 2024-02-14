namespace Wave.Utilities;

public interface IMessageDisplay {
	public void ShowMessage(Message message);
	
	public void ShowInfo(string message, string? title = null)
		=> ShowMessage(new Message(message, "alert-info", title, DateTimeOffset.UtcNow));
	public void ShowSuccess(string message, string? title = null)
		=> ShowMessage(new Message(message, "alert-success", title, DateTimeOffset.UtcNow));
	public void ShowWarning(string message, string? title = null)
		=> ShowMessage(new Message(message, "alert-warning", title, DateTimeOffset.UtcNow));
	public void ShowError(string message, string? title = null)
		=> ShowMessage(new Message(message, "alert-error", title, DateTimeOffset.UtcNow));

	public sealed record Message(string Body, string Type, string? Title, DateTimeOffset Created);
}