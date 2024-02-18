using System.Collections.Frozen;
using Microsoft.Extensions.Hosting;
using Mjml.Net.Helpers;

namespace Wave.Services;

public record StaticEmail(string ReceiverEmail, string? ReceiverName, string Subject, string Title, string ContentHtml, 
	FrozenDictionary<string, string> Headers) : IEmail;