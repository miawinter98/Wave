namespace Wave.Utilities;

public interface IMessageDisplay {
	IReadOnlyList<Message> GetMessages();
	event Func<Message, bool>? OnMessage;

	Message? Pop();

	void ShowMessage(Message message);
	
	void ShowInfo(string message, string? title = null)
		=> ShowMessage(new Message(message, "alert-info", title, DateTimeOffset.UtcNow));
	void ShowSuccess(string message, string? title = null)
		=> ShowMessage(new Message(message, "alert-success", title, DateTimeOffset.UtcNow));
	void ShowWarning(string message, string? title = null)
		=> ShowMessage(new Message(message, "alert-warning", title, DateTimeOffset.UtcNow));
	void ShowError(string message, string? title = null)
		=> ShowMessage(new Message(message, "alert-error", title, DateTimeOffset.UtcNow));

	sealed record Message(string Body, string Type, string? Title, DateTimeOffset Created);
}