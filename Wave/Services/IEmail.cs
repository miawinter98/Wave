using System.Collections.Frozen;

namespace Wave.Services;

public interface IEmail {
	string ReceiverEmail { get; }
	string? ReceiverName { get; }
	string Subject { get; }
	string Title { get; }
	string ContentHtml { get; }
	string ContentPlain { get; }
	FrozenDictionary<string, string> Headers { get; }
}