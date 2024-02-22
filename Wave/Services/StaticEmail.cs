using System.Collections.Frozen;

namespace Wave.Services;

public record StaticEmail(string ReceiverEmail, string? ReceiverName, string Subject, string Title, string ContentHtml, string ContentPlain, FrozenDictionary<string, string> Headers) : IEmail;