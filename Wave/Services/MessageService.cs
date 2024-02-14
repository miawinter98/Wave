using Wave.Utilities;
using static Wave.Utilities.IMessageDisplay;

namespace Wave.Services;

public class MessageService : IMessageDisplay {
	private Queue<Message> Messages { get; } = new();

	public event Func<Message, bool>? OnMessage;

	public IReadOnlyList<Message> GetMessages() => [.. Messages];
	public Message? Pop() => Messages.TryDequeue(out var m) ? m : null;

	public void ShowMessage(Message message) {
		if (OnMessage?.Invoke(message) is not true) {
			Messages.Enqueue(message);
		}
	}
}